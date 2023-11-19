namespace github_whitelist {
    public abstract class Error {
        public abstract string Message { get; }
    }

    public class NotFoundError(string entityType) : Error {
        public override string Message => $"could not retrieve {entityType}";

        public override string ToString() {
            return Message;
        }
    }
}