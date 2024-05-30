using FluentEmail.Core;
using LoginRegistrationTask.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace LoginRegistrationTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString = @"Data Source=DESKTOP-UDPGKSQ\SQLEXPRESS01;Initial Catalog=DigiLink;Integrated Security=True";

       
        private readonly string _smtpServer = "sandbox.smtp.mailtrap.io";
        private readonly int _smtpPort = 2525;
        private readonly string _smtpUsername = "a1ddc8a4c09c1d";
        private readonly string _smtpPassword = "06703361124f58";

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            try
            {
                var user = new User
                {
                    Name = userDto.Name,
                    Email = userDto.Email,
                    Password = userDto.Password,
                    Phone = userDto.Phone
                };

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var command = new SqlCommand(
                        "INSERT INTO Users (Name, Email, Password, Phone) OUTPUT INSERTED.Id VALUES (@Name, @Email, @Password, @Phone)",
                        connection);

                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@Email", user.Email); 
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.Parameters.AddWithValue("@Phone", user.Phone);

                    user.Id = (int)await command.ExecuteScalarAsync();
                }

                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    client.EnableSsl = true;
                    

                    var mailMessage = new MailMessage("dharmendra15oct1998@gmail.com", user.Email, "Welcome!", $"Hello {user.Name}, welcome to our service!");

                    await client.SendMailAsync(mailMessage);
                }

                return Ok("Successfully Registration.");
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"SMTP Exception: {smtpEx.Message}");
                Console.WriteLine($"Inner Exception: {smtpEx.InnerException?.Message}");
                return StatusCode(500, "An error occurred while sending the email.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(Login loginDto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var command = new SqlCommand(
                        "SELECT Id, Name, Email FROM Users WHERE Email = @Email AND Password = @Password",
                        connection);

                    command.Parameters.AddWithValue("@Email", loginDto.Email);
                    command.Parameters.AddWithValue("@Password", loginDto.Password);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();
                            var userId = reader.GetInt32(0);
                            var name = reader.GetString(1);
                            var email = reader.GetString(2);

                            // You can include additional user data if needed

                            return Ok("Login successfully.");
                        }
                        else
                        {
                            return BadRequest("Invalid email or password.");
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

    }
}
//private readonly string _connectionString;
//private readonly IFluentEmail _email;
//private readonly IBackgroundTaskQueue _taskQueue;

//public UserController(IConfiguration configuration, IFluentEmail email, IBackgroundTaskQueue taskQueue)
//{
//    _connectionString = configuration.GetConnectionString("Conn");
//    _email = email;
//    _taskQueue = taskQueue;
//}

//[HttpPost]
//public async Task<IActionResult> Register(UserDto userDto)
//{
//    var user = new User
//    {
//        Name = userDto.Name,
//        Email = userDto.Email,
//        Password = userDto.Password,
//        Phone = userDto.Phone
//    };

//    try
//    {
//        using (var connection = new SqlConnection(_connectionString))
//        {
//            await connection.OpenAsync();

//            var command = new SqlCommand(
//                "INSERT INTO Users (Name, Email, Password, Phone) OUTPUT INSERTED.Id VALUES (@Name, @Email, @Password, @Phone)",
//                connection);

//            command.Parameters.AddWithValue("@Name", user.Name);
//            command.Parameters.AddWithValue("@Email", user.Email);
//            command.Parameters.AddWithValue("@Password", user.Password);
//            command.Parameters.AddWithValue("@Phone", user.Phone);

//            System.Console.WriteLine("Sent1");
//            user.Id = (int)await command.ExecuteScalarAsync();
//        }

//        var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
//        {
//            Credentials = new NetworkCredential("a1ddc8a4c09c1d", "********4f58"),
//            EnableSsl = true
//        };

//        // Await the SendAsync method to catch any exceptions
//        await client.SendMailAsync("dharmendra15oct1998@gmail.com", "dhirendrapratap28maurya@gmail.com", "Hello world", "testbody");
//        System.Console.WriteLine("Sent");

//        // Your remaining code for sending background email

//        return Ok(new { user.Id });
//    }
//    catch (Exception ex)
//    {
//        // Log the exception
//        System.Console.WriteLine($"Error sending email: {ex.Message}");
//        // Return an error response
//        return StatusCode(500, "An error occurred while processing your request.");
//    }
//}



//[HttpPost("login")]
//        public async Task<IActionResult> Login(Login login)
//        {
//            User user = null;
//            using (var connection = new SqlConnection(_connectionString))
//            {
//                await connection.OpenAsync();

//                var command = new SqlCommand("SELECT  Email, Password, Phone FROM Users WHERE Email = @Email AND Password = @Password", connection);
//                command.Parameters.AddWithValue("@Email", login.Email);
//                command.Parameters.AddWithValue("@Password", login.Password);

//                using (var reader = await command.ExecuteReaderAsync())
//                {
//                    if (!reader.HasRows)
//                    {
//                        return Unauthorized("Invalid email or password.");
//                    }

//                    await reader.ReadAsync();
//                    user = new User
//                    {
//                        Id = reader.GetInt32(0),
//                        Name = reader.GetString(1),
//                        Email = reader.GetString(2),
//                        Password = reader.GetString(3),
//                        Phone = reader.GetString(4)
//                    };
//                }
//            }

//            return Ok(new
//            {
//                Message = "Login successful",
//                User = new
//                {
//                    user.Id,
//                    user.Name,
//                    user.Email,
//                    user.Phone
//                }
//            });
//        }
//    }
//}
