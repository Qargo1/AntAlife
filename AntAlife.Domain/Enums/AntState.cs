namespace AntAlife.Domain.Enums
{
    public enum AntState // Или оставим EntityState, если подходит
    {
        Idle,             // Просто стоит (или отдыхает)
        Wandering,        // Бесцельно бродит (замена простому Exploring?)
        SearchingForFood, // Активно ищет еду (может включать Wandering)
        FollowingFoodTrail, // Идет по следу феромона еды <--- Новое!
        ReturningToNest,  // Возвращается в гнездо (с едой или без)
        CarryingFood,     // Несет еду (часто совмещено с ReturningToNest)
        CarryingItem,
        Fighting,         // Сражается
        Fleeing,          // Убегает от опасности
        FollowingDangerTrail, // Убегает от феромона опасности <--- Новое!
        TendingToEggs,    // Ухаживает за яйцами (для нянек)
        Digging,          // Копает (если добавим)
        Building,          // Строит (если добавим)
        WaitingForNest    // Муравейника нет, или он разрушен?
        // ... можно добавить еще!
    }
}