namespace Bedrock.Core.ValueObjects
{
    // 地址值对象
    public class Address
    {
        public string Street { get; private set; }
        public string City { get; private set; }

        public Address(string street, string city)
        {
            Street = street ?? throw new ArgumentNullException(nameof(street));
            City = city ?? throw new ArgumentNullException(nameof(city));
        }
    }
}
