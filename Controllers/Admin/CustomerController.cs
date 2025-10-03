using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using MySqlConnector;
using WebApplication3.Datas;
using WebApplication3.DTOs;
using WebApplication3.DTOs.CustomerDTOs;
using WebApplication3.DTOs.UserDTO;
using WebApplication3.Helpers;
using WebApplication3.Middlewares;
using WebApplication3.Models;
using static System.Net.WebRequestMethods;

namespace WebApplication3.Controllers.Admin                                 //*********************
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private FleetDB _context;
        private MySqlConnection _conn;
        public CustomerController(FleetDB context, MySqlConnection conn)
        {
            _context = context; 
            _conn = conn;   
        }

        [JwtAuthMiddleware]
        [HttpPost("createCustomer")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDTO customerDTO)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin.", Data = null });
                }

                var validateData = ValidateString.StringValidator(
                    new Dictionary<string, string>{
                        { "Mã khách hàng", customerDTO.CustomerCode },
                        { "Tên khách hàng", customerDTO.CustomerName },
                        { "Địa chỉ khách hàng", customerDTO.CustomerAddr },
                        { "Mã số thuế khách hàng", customerDTO.TaxNo },
                        { "Tên liên hệ khách hàng", customerDTO.ContactName },
                        { "Số liên hệ khách hàng", customerDTO.ContactPhone },
                        { "ID người dùng", customerDTO.UserID.ToString() },
                });
                if (validateData != null)
                {
                    return Ok(validateData);
                }
                bool existsCode = await _context.Customers
                    .AnyAsync(c => c.CustomerCode == customerDTO.CustomerCode);
                bool existsTaxNo = await _context.Customers
                    .AnyAsync(c => c.TaxNo == customerDTO.TaxNo);
                var existsUser = await _context.User.AnyAsync(u => u.UserID == customerDTO.UserID);
                if (!existsUser)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Khách hàng '{customerDTO.UserID}' không tồn tại.", Data = null });
                if (existsCode)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Mã khách hàng '{customerDTO.CustomerCode}' đã tồn tại.", Data = null });
                if (existsTaxNo)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"⚠️ Mã số thuế khách hàng đã trùng với Khách hàng nào đó. Mã '{customerDTO.TaxNo}' đã tồn tại.", Data = null });

                var customer = new Customer
                {
                    CustomerCode = customerDTO.CustomerCode.ToUpper(),
                    CustomerName = customerDTO.CustomerName.ToUpper(),
                    CustomerAddr = customerDTO.CustomerAddr,
                    DisplayOrder = customerDTO.DisplayOrder,
                    TaxNo        = customerDTO.TaxNo,
                    ContactName  = customerDTO.ContactName.ToUpper(),
                    ContactPhone = customerDTO.ContactPhone,
                    UserID = customerDTO.UserID,
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return Ok(new ApiResponse<Object> {
                    statusCode = 200,
                    Message = $"✅ Tạo khách hàng '{customer.CustomerName}' thành công.",
                    Data = customer
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpPut("updateCustomer/{id}")]
        public async Task<IActionResult> UpdateCustomer(int id ,[FromBody] UpdateCustomerDTO customerDTO)
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Bạn không có quyền Admin.", Data = null });
                }

                var validateData = ValidateString.StringValidator(
                    new Dictionary<string, string>{
                        { "Mã khách hàng", customerDTO.CustomerCode },
                        { "Tên khách hàng", customerDTO.CustomerName },
                        { "Địa chỉ khách hàng", customerDTO.CustomerAddr },
                        { "Mã số thuế khách hàng", customerDTO.TaxNo },
                        { "Tên liên hệ khách hàng", customerDTO.ContactName },
                        { "Số liên hệ khách hàng", customerDTO.ContactPhone }
                });
                if (validateData != null)
                {
                    return Ok(validateData);
                }

                // Tìm khách hàng cần update
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == id);
                if (customer == null)
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = $"Khách hàng '{customerDTO.CustomerCode}' không có.", Data = null });

                // Kiểm tra trùng mã khách hàng (trừ chính nó)
                bool codeExists = await _context.Customers
                    .AnyAsync(c => c.CustomerCode == customerDTO.CustomerCode && c.CustomerID != id);

                if (codeExists)
                    return Ok(new ApiResponse<string> { statusCode = 409, Message = $"Mã khách hàng '{customerDTO.CustomerCode}' đã tồn tại.", Data = null });

                // Cập nhật thông tin
                customer.CustomerCode = customerDTO.CustomerCode.ToUpper();
                customer.CustomerName = customerDTO.CustomerName.ToUpper();
                customer.CustomerAddr = customerDTO.CustomerAddr;
                customer.TaxNo        = customerDTO.TaxNo;
                customer.ContactName  = customerDTO.ContactName.ToUpper();
                customer.ContactPhone = customerDTO.ContactPhone;

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                
                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"✅ Cập nhật khách hàng '{customer.CustomerName}' thành công.",
                    Data = customer
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpGet("listCustomer")]
        public async Task<IActionResult> ListCustomer(
                [FromQuery] string order = "asc", // ✅ Default value
                [FromQuery] string sortBy = "id", // ✅ Default value  
                [FromQuery] int pageSize = 10,    // ✅ Default value
                [FromQuery] string keySearch = "",    // ✅ Default value
                [FromQuery] int pageNumber = 1    // ✅ Default value

        )
        {
            try
            {
                //var adminOrEntry = FilterEntryOrAdmin.CheckEntryOrAdmin(HttpContext);
                //if (adminOrEntry != null)
                //{
                //    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin hoặc Entry.", Data = null });
                //}

                var query = _context.Customers
                    .Where(c => c.IsActive == true
                        && ( string.IsNullOrEmpty(keySearch)
                        || c.CustomerID.ToString().Contains(keySearch)
                        || c.ContactName.Contains(keySearch)
                        || c.CustomerName.Contains(keySearch)
                        || c.ContactPhone.Contains(keySearch)
                        || c.CustomerCode.Contains(keySearch)
                ));

                switch(sortBy.ToLower())
                {
                    case "name": 
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(c => c.CustomerName)
                            : query.OrderBy(c => c.CustomerName);
                        break;                        
                    
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(c => c.CustomerID)
                            : query.OrderBy(c => c.CustomerID);
                        break;
                }

                // Phân trang
                int page = Math.Max(1, pageNumber); // tránh page < 1
                int size = Math.Max(1, pageSize);   // tránh size < 1

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);

                var listCustomer = await query
                    .Select(c => new { c.CustomerID, c.CustomerName, c.CustomerCode , c.ContactPhone, c.CustomerAddr })
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Danh sách khách hàng.",
                    Data = listCustomer
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }



        [JwtAuthMiddleware]
        [HttpGet("detailCustomer")]
        public async Task<IActionResult> DetailCustomer(
            [FromQuery] int id
        )
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin.", Data = null });
                }
                if (id == null || id <= 0)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = ID Customer không hợp lệ.", Data = null });
                }
                //string sql = @"
                //            Select * 
                //            FROM  tblCustomer AS c
                //            INNER JOIN tbluser AS u
                //                ON c.UserID = u.UserID
                //            WHERE c.CustomerID = {0}

                //"; 
                //var detailUser = await _context.Customers.FromSqlRaw(sql, id).FirstOrDefaultAsync();  //lấy bản ghi đầu tiên nếu có, nếu không thì trả về null.

                var detailUser = await _context.Customers
                    .Where(c => c.CustomerID == id)
                    .Select(c => new
                    {
                        c.CustomerID ,
                        c.CustomerCode,
                        c.CustomerName,
                        c.ContactName,
                        c.CustomerAddr,
                        c.IsActive,
                        c.TaxNo,
                        c.UserID,
                        c.ContactPhone,
                        User = new User
                        {
                            Password = c.User.Password,
                            UserName = c.User.UserName,
                        }
                    }
                    ).FirstOrDefaultAsync();


                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Thông tin chi tiết khách hàng '{detailUser.CustomerName}' thành công.",
                    Data = detailUser
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }


        //not use
        [JwtAuthMiddleware]
        [HttpGet("searchCustomer")]
        public async Task<IActionResult> SearchCustomer(
                [FromQuery] string keySearch,
                [FromQuery] string order = "asc", // ✅ Default value
                [FromQuery] string sortBy = "id", // ✅ Default value  
                [FromQuery] int pageSize = 10,    // ✅ Default value
                [FromQuery] int pageNumber = 1    // ✅ Default value
        )
        {
            try
            {
                var checkAdmin = FilterAdmin.CheckAdmin(HttpContext);
                if (checkAdmin != null)
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "⚠️ = Bạn không có quyền Admin.", Data = null });
                }

                if (string.IsNullOrEmpty(keySearch))
                {
                    return Ok(new ApiResponse<string> { statusCode = 400, Message = "Nhập vào từ khóa tìm kiếm", Data = null });
                }

                var query = _context.Customers.Where(c => c.IsActive == true && (string.IsNullOrEmpty(keySearch) || c.CustomerName.Contains(keySearch.ToUpper()) || c.CustomerCode.Contains(keySearch.ToUpper())));

                switch (sortBy.ToLower())
                {
                    case "name":
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(c => c.CustomerName)
                            : query.OrderBy(c => c.CustomerName);
                        break;
                    default:
                        query = order.ToLower() == "desc"
                            ? query.OrderByDescending(c => c.CustomerID)
                            : query.OrderBy(c => c.CustomerID);
                        break;
                }

                // Phân trang
                int page = Math.Max(1, pageNumber); // tránh page < 1
                int size = Math.Max(1, pageSize);   // tránh size < 1

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)size);

                var listCustomer = await query
                    .Select(c => new { c.CustomerID, c.CustomerName, c.CustomerCode })
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();

                return Ok(new ApiResponse<Object>
                {
                    statusCode = 200,
                    Message = $"Danh sách tìm kiếm khách hàng.",
                    Data = listCustomer
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string> { statusCode = 500, Message = $"Lỗi server: {ex.Message}", Data = null });
            }
        }

       
    }
}
