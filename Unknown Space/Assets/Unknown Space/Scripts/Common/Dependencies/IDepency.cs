/// <summary>
/// Обобщенный интерфейс для внедрения зависимостей.
/// </summary>
/// <typeparam name="T">Тип объекта.</typeparam>
public interface IDependency<T>
{
    /// <summary>
    /// Передача ссылки на объект.
    /// </summary>
    /// <param name="obj"></param>
    void Construct(T obj);
}