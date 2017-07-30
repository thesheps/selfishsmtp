# selfishsmtp
Self-hosted smtp server for .net core testing

Does exactly that.


```csharp
[Test]
public void WhenIDoAThing_ThenAnEmailIsSent() 
{
  using(var smtpServer = new SmtpServer(25)
  {
    var thingerer = new Thinger();
    thingerer.SendEmail("from@place.com", "to@otherplace.com", "This is an email!");
    
    Assert.That(smtpServer.Messages.Count, Is.EqualTo(1));
  }
}
```
