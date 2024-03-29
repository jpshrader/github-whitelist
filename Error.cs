namespace github_whitelist {
    public abstract class Error {
        public abstract string Message { get; }

        public override string ToString() {
            return Message;
        }
    }

    public class NotFoundError(string entityType) : Error {
        public override string Message => $"could not retrieve {entityType}";
    }

    public class CoundNotDeleteError(string entityType, string id) : Error {
        public override string Message => $"could not delete {entityType}: {id}";

        public string Id => id;
    }

    public class CoundNotCreateError(string entityType, string id) : Error {
        public override string Message => $"could not create {entityType}: {id}";

        public string Id => id;
    }
}