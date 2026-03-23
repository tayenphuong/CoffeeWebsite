using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanNuocMVC.Data;
using WebBanNuocMVC.DesignPatterns.Command;
using WebBanNuocMVC.DesignPatterns.Observer;
using WebBanNuocMVC.DesignPatterns.State;

namespace WebBanNuocMVC.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderSubject _orderSubject;
        private readonly CoffeeShopDbContext _context;
        private readonly OrderCommandInvoker _invoker; // Dùng biến private readonly
        private readonly IServiceScopeFactory _scopeFactory; // Thêm cái này để xử lý Singleton

        public OrdersController(CoffeeShopDbContext context,
                             IOrderSubject orderSubject,
                             OrderCommandInvoker invoker, // Lấy từ DI
                             IServiceScopeFactory scopeFactory) // Lấy từ DI
        {
            _context = context;
            _orderSubject = orderSubject;
            _invoker = invoker; // Gán từ DI, không dùng 'new'
            _scopeFactory = scopeFactory;
        }

        public async Task<IActionResult> Index(string status = "all")
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .AsQueryable();

            status = (status ?? "all").Trim().ToLower();

            switch (status)
            {
                case "pendingpayment":
                    query = query.Where(o =>
                        o.Status == "PendingPayment" ||
                        o.Status == "Pending Payment");
                    break;

                case "pending":
                    query = query.Where(o =>
                        o.Status == "Pending" ||
                        o.Status == "Processing");
                    break;

                case "completed":
                    query = query.Where(o =>
                        o.Status == "Completed" ||
                        o.Status == "Paid");
                    break;

                case "cancelled":
                    query = query.Where(o =>
                        o.Status == "Cancelled" ||
                        o.Status == "Canceled");
                    break;

                case "all":
                default:
                    break;
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.CanUndo = _invoker.CanUndo;
            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Account)
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult Create()
        {
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            ViewData["TableId"] = new SelectList(_context.CafeTables, "TableId", "TableId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,TableId,CustomerId,AccountId,OrderDate,TotalAmount,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId", order.AccountId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["TableId"] = new SelectList(_context.CafeTables, "TableId", "TableId", order.TableId);
            return View(order);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId", order.AccountId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["TableId"] = new SelectList(_context.CafeTables, "TableId", "TableId", order.TableId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,TableId,CustomerId,AccountId,OrderDate,TotalAmount,Status")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountId", order.AccountId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["TableId"] = new SelectList(_context.CafeTables, "TableId", "TableId", order.TableId);
            return View(order);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Account)
                .Include(o => o.Customer)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }

        private async Task<bool> ChangeOrderState(int orderId, OrderAction action)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return false;

            var oldStatus = order.Status ?? string.Empty;

            var context = new OrderContext(order.Status);
            context.Handle(action);

            order.Status = context.CurrentStatus;

            await _context.SaveChangesAsync();

            await _orderSubject.NotifyAsync(new OrderStatusChangedEvent
            {
                OrderId = order.OrderId,
                OldStatus = oldStatus,
                NewStatus = order.Status ?? string.Empty,
                ChangedAt = DateTime.Now,
                CustomerEmail = order.Customer?.Email
            });

            return true;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPreparing(int id)
        {
            try
            {
                // Đóng gói logic vào Command
                // Ví dụ cho MarkPreparing
                var command = new OrderStatusCommand(_scopeFactory, ChangeOrderState, id, OrderAction.StartPreparing);

                // Gửi lệnh đi thực thi thông qua Invoker
                await _invoker.ExecuteCommandAsync(command);

                TempData["Success"] = "Đơn hàng đã chuyển sang trạng thái Preparing.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            try
            {
                // Truyền _scopeFactory thay vì _context
                var command = new OrderStatusCommand(_scopeFactory, ChangeOrderState, id, OrderAction.Complete);

                await _invoker.ExecuteCommandAsync(command);

                TempData["Success"] = "Đơn hàng đã hoàn tất thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                // 1. Khởi tạo Command cho hành động Hủy (Cancel)
                var command = new OrderStatusCommand(_scopeFactory, ChangeOrderState, id, OrderAction.Cancel);

                // 2. Gửi lệnh cho Invoker thực thi
                await _invoker.ExecuteCommandAsync(command);

                TempData["Success"] = "Đơn hàng đã được hủy.";
            }
            catch (Exception ex)
            {
                // Nếu State Pattern báo lỗi (vd: Đơn đã giao không được hủy), lỗi sẽ bắn về đây
                TempData["Error"] = "Lỗi khi hủy đơn: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Undo()
        {
            try
            {
                string desc = await _invoker.UndoLastCommandAsync();
                if (desc != null)
                    TempData["Success"] = $"Đã hoàn tác: {desc}";
                else
                    TempData["Error"] = "Không còn lệnh nào để hoàn tác.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi hoàn tác: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
