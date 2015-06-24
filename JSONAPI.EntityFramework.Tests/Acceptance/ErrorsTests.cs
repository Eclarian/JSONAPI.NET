using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.EntityFramework.Tests.Acceptance
{
    [TestClass]
    public class ErrorsTests : AcceptanceTestsBase
    {
        [TestMethod]
        public async Task Controller_action_throws_exception()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "trees");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Errors\Controller_action_throws_exception.json", HttpStatusCode.InternalServerError, true);
            }
        }

        [TestMethod]
        public async Task Controller_does_not_exist()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "foo");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Errors\Controller_does_not_exist.json", HttpStatusCode.NotFound, true);
            }
        }
    }
}
