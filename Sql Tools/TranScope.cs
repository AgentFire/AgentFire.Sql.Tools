using System.Transactions;

namespace AgentFire.Sql.Tools
{
    internal sealed class TranScope : DisposableObject
    {
        public static readonly IsolationLevel IsolationLevel = IsolationLevel.Serializable;
        private static readonly TransactionOptions _options = new TransactionOptions() { IsolationLevel = IsolationLevel };

        private readonly TransactionScope _scope = new TransactionScope(TransactionScopeOption.Required, _options, TransactionScopeAsyncFlowOption.Enabled);

        public void Commit() => _scope.Complete();
        protected override void DisposeInternal() => _scope.Dispose();
    }
}
