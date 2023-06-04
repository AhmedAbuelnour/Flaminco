namespace Flaminco.Results;

public enum FailureType {
    Invalid = 1,
    Unauthorized,
    NotFound,
    InvalidValidation,
    ServerException,
    Duplicated,
    Others
}