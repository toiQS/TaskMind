using System.Windows;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace TaskMind.WPFs.Utilities
{
    /// <summary>
    /// Cho phép ViewModel lấy IMediator mà không phá vỡ các constructor "new XxxVM()"
    /// đang dùng khắp AdminNavigationVM/CompanyVM/SchoolVM (design-time, điều hướng nội bộ).
    /// Ưu tiên IMediator được truyền tay (test/inject rõ ràng); nếu null thì resolve từ DI container
    /// của App hiện hành. Đây là compromise thực dụng — lý tưởng nhất về sau nên có 1 VM Factory
    /// injected xuống toàn chuỗi điều hướng thay vì "new" trực tiếp.
    /// </summary>
    public static class MediatorResolver
    {
        public static IMediator Resolve(IMediator injected)
        {
            if (injected != null) return injected;
            return (Application.Current as App)?.ServiceProvider.GetService<IMediator>();
        }
    }
}