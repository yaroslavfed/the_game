using Microsoft.Extensions.Logging.Abstractions;
using the_game.Lifecycle;
using Xunit;

namespace the_game.Tests.Lifecycle;

public sealed class ApplicationErrorServiceTests
{
    [Fact]
    public void OnNext_ShowsSafeMessage_AndClearDismissesIt()
    {
        var service = new ApplicationErrorService(NullLogger<ApplicationErrorService>.Instance);

        service.OnNext(new InvalidOperationException("Sensitive technical details"));

        Assert.Equal("Операция не выполнена. Попробуйте ещё раз.", service.Message);

        service.Clear();

        Assert.Null(service.Message);
    }
}
