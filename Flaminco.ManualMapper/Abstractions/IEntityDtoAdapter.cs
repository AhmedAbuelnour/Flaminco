namespace Flaminco.ManualMapper.Abstractions
{
    public interface IEntityDtoAdapter<TEntity, TDto> where TEntity : class, new() where TDto : class, new()
    {
        TDto ToDto(TEntity entity);
        TEntity ToEntity(TDto dto);
    }

}
