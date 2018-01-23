namespace ShoppingList.Dtos
{
    public class ShoppingItemDto
    {
        public string Id { get; set; }
        public string Item { get; set; }
        public UserDto User { get; set; }
        public bool Completed { get; set; }
    }
}