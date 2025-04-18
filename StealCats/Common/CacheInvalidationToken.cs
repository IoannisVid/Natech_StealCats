namespace StealTheCats.Common
{
    public class CacheInvalidationToken
    {
        private CancellationTokenSource _resetToken = new();

        public CancellationToken Token => _resetToken.Token;

        public void Invalidate()
        {
            var previousToken = _resetToken;
            _resetToken = new CancellationTokenSource();

            previousToken.Cancel();
            previousToken.Dispose();
        }
    }
}
