namespace WebBanNuocMVC.DesignPatterns.Command
{
    public class OrderCommandInvoker
    {
        private readonly Stack<IOrderCommand> _history = new Stack<IOrderCommand>();

        public async Task ExecuteCommandAsync(IOrderCommand command)
        {
            await command.ExecuteAsync();
            _history.Push(command); // Đẩy vào ngăn xếp lịch sử

            Console.WriteLine($"Stack count: {_history.Count}");
        }

        public async Task<string> UndoLastCommandAsync()
        {
            if (_history.Count > 0)
            {
                var command = _history.Pop();
                await command.UndoAsync();
                return command.Description;
            }
            return null;
        }

        public bool CanUndo => _history.Count > 0;
    }
}
