using mozgovichok.Models.Courses;
using mozgovichok.Models.Courses.Statistics;
using mozgovichok.Models.DTO;

namespace mozgovichok.Infrastructure
{
    public static class StatsHandler
    {
        public static OrderStatistic CreateOrderStats(VROrderStatistic vrStats)
        {
            OrderStatistic orderStatistic = new OrderStatistic();
            orderStatistic.OrderErrors = vrStats.OrderErrors ?? 0;
            orderStatistic.OrderTime = vrStats.OrderTime ?? 0;
            orderStatistic.LatencyTime = vrStats.LatencyTime ?? 0;
            orderStatistic.AverageLatencyTime = vrStats.AverageLatencyTime ?? 0;
            orderStatistic.VoiceLatencyTime = vrStats.VoiceLatencyTime ?? 0;
            orderStatistic.ImpulseAction = vrStats.ImpulseAction ?? 0;

            return orderStatistic;
        }

        public static ExerciseStatistic CountExerciseStats(Exercise exercise)
        {
            ExerciseStatistic stats = new ExerciseStatistic() 
            { 
                ExerciseErrors = 0 
                , AverageLatencyTime = 0
                , ExerciseTime = 0
                , ImpulseAction = 0
                , LatencyTime = 0
                , VoiceLatencyTime = 0
            };
            int finishedOrders = 0;
            foreach(Order o in exercise.Orders)
            {
                if (o.OrderStatistic != null)
                {
                    stats.ExerciseErrors += o.OrderStatistic.OrderErrors;
                    stats.ExerciseTime += o.OrderStatistic.OrderTime;
                    stats.ImpulseAction += o.OrderStatistic.ImpulseAction;
                    finishedOrders++;
                    stats.AverageLatencyTime += o.OrderStatistic.AverageLatencyTime;
                }
            }
            stats.AverageLatencyTime /= finishedOrders;
            return stats;
        }

    }

}
