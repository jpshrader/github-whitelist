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

    public class CoundNotDeleteError(string entityType, string id) : Error {
        public override string Message => $"could not delete {entityType}: {id}";

        public string Id => id;

        public override string ToString() {
            return Message;
        }
    }
}