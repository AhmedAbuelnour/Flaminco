using Flaminco.DualMapper.Abstractions;

namespace WebApplication1.Validations
{

    public class Entity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Dto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }

    public class AdaptorExampleMapper : IDualMapper<Entity, Dto>
    {
        public Dto MapTo(Entity entity)
        {
            return new Dto
            {
                Id = entity.Id,
                FullName = $"{entity.FirstName} {entity.LastName}"
            };
        }

        public Entity MapFrom(Dto dto)
        {
            return new Entity
            {
                Id = dto.Id,
                FirstName = dto.FullName.Split(' ')[0],
                LastName = dto.FullName.Split(' ')[1]
            };
        }
    }


}
