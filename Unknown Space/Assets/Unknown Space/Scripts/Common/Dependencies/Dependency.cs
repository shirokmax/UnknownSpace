using UnityEngine;

public abstract class Dependency : MonoBehaviour
{
    protected abstract void BindAll(MonoBehaviour monoBehaviourInScene);

    /// <summary>
    /// Метод связывания ссылок.
    /// Если монобех реализует интерфейс с нужным типом, то вызываем метод передачи ссылки на нужный объект
    /// </summary>
    /// <param name="mono"></param>
    protected void Bind<T>(MonoBehaviour bindObject, MonoBehaviour target) where T : class
    {
        (target as IDependency<T>)?.Construct(bindObject as T);
    }

    protected void FindAllObjectsToBind()
    {
        // Массив всех монобехов, находящихся на сцене
        MonoBehaviour[] monosInScene = FindObjectsOfType<MonoBehaviour>(true);

        // Обработка всех монобехов на сцене.
        // Для определения очередности необходимо делать отдельный такой цикл и ставить его в нужном порядке по очередности относительно других циклов.
        foreach (var mono in monosInScene)
            BindAll(mono);
    }
}
