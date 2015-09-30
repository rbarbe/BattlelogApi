using System.Threading.Tasks;
using Xunit;

namespace Battlelog.BattlelogApi.Test
{
    public class Functional
    {

        [Fact]
        public async Task Test()
        {
            var username = "<username>";
            var password = "<password>";
            var api = new Api();
            await api.SignIn(username, password);
            var missions = await api.GetCommunityMissions(Models.BattlelogGame.BFH);
            Assert.NotNull(missions);
        }
    }
}
