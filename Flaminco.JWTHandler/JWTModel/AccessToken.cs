namespace Flaminco.JWTHandler.JWTModel;
public class AccessToken
{
    public string Token { get; set; }
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; }
}
