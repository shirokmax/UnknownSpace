/// <summary>
/// ���������� ��������� ��� ��������� ������������.
/// </summary>
/// <typeparam name="T">��� �������.</typeparam>
public interface IDependency<T>
{
    /// <summary>
    /// �������� ������ �� ������.
    /// </summary>
    /// <param name="obj"></param>
    void Construct(T obj);
}