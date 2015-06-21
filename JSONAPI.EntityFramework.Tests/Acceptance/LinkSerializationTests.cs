using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.EntityFramework.Tests.Acceptance
{
    [TestClass]
    public class LinkSerializationTests : AcceptanceTestsBase
    {
        [TestMethod]
        public async Task Get_resource_with_related_link_and_missing_to_one_relationship()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "cities/9001");

                await AssertResponseContent(response, @"Acceptance\Fixtures\LinkSerialization\Responses\GetResourceWithRelatedLinkAndMissingToOneRelationshipResponse.json", HttpStatusCode.OK);
            }
        }

        [TestMethod]
        public async Task Get_resource_with_related_link_and_present_to_one_relationship()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "cities/9000");

                await AssertResponseContent(response, @"Acceptance\Fixtures\LinkSerialization\Responses\GetResourceWithRelatedLinkAndPresentToOneRelationshipResponse.json", HttpStatusCode.OK);
            }
        }
    }
}
