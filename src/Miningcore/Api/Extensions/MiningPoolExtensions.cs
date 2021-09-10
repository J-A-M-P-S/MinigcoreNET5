using System.Linq;
using AutoMapper;
using Miningcore.Api.Responses;
using Miningcore.Blockchain;
using Miningcore.Configuration;
using Miningcore.Extensions;
using Miningcore.Mining;

namespace Miningcore.Api.Extensions
{
    public static class MiningPoolExtensions
    {
        public static PoolInfo ToPoolInfo(this PoolConfig poolConfig, IMapper mapper, Persistence.Model.PoolStats stats, IMiningPool pool)
        {
            var poolInfo = mapper.Map<PoolInfo>(poolConfig);

            poolInfo.PoolStats = mapper.Map<PoolStats>(stats);
            poolInfo.NetworkStats = pool?.NetworkStats ?? mapper.Map<BlockchainStats>(stats);

            // pool wallet link
            var addressInfobaseUrl = poolConfig.Template.ExplorerAccountLink;
            if(!string.IsNullOrEmpty(addressInfobaseUrl))
                poolInfo.AddressInfoLink = string.Format(addressInfobaseUrl, poolInfo.Address);

            // pool fees
            poolInfo.PoolFeePercent = poolConfig.RewardRecipients != null ? (float) poolConfig.RewardRecipients.Sum(x => x.Percentage) : 0;

            // strip security critical stuff
            if(poolInfo.PaymentProcessing.Extra != null)
            {
                var extra = poolInfo.PaymentProcessing.Extra;

                //extra.StripValue(nameof(EthereumPoolPaymentProcessingConfigExtra.CoinbasePassword));
            }

            return poolInfo;
        }

        public static decimal CalculatePercentage(this PoolConfig poolConfig, string adress)
        {
            var selfAdress = poolConfig.Address;
            var result = 0m;
            var customPercentage = poolConfig?.RewardRecipients?
                .Where(x => x.Address != selfAdress)
                .Where(x => x.CustomAddresses.Contains(adress))
                .Sum(x => x.CustomPercentage);

            if(customPercentage.HasValue)
            {
                result += customPercentage.Value;
            }

            var percentage = poolConfig?.RewardRecipients?
                .Where(x => x.Address != selfAdress)
                .Sum(x => x.Percentage);

            if(percentage.HasValue)
            {
                result += percentage.Value;
            }

            return result;
        }        
    }
}
