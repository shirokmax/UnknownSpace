using UnityEngine;

namespace UnknownSpace
{
    [RequireComponent (typeof (Camera))]
    public class CameraController : MonoBehaviour, IDependency<SpaceShip>,IDependency<UITargetSelection>
    {
        #region Properties
        /// <summary>
        /// Корневой пустой объект камеры. (нужен для закрепления движения камеры по оси Z)
        /// </summary>
        [SerializeField] private Transform m_CamTargetRoot;
        /// <summary>
        /// Родитель камеры, также являющийся дочерним к camTargetRoot. (нужен для правильного движения камеры вокруг корабля)
        /// </summary>
        [SerializeField] private Transform m_CamTargetParent;
        /// <summary>
        /// Место расположения самой камеры.
        /// </summary>
        [SerializeField] private Transform m_CamTarget;
        /// <summary>
        /// Ссылка на класс, через который получаем переменную, хранящую позицию выделенной цели.
        /// </summary>

        /// <summary>
        /// Переключаемый режим камеры. (Свободная камера, следование за кораблем, следование за целью)
        /// </summary>
        [Space(20)] public CameraMode m_CameraMode;
        /// <summary>
        /// Время, через которое включается режим камеры после бездействия движения мышки.
        /// </summary>
        [SerializeField][Min(0)] private float m_CameraModeTimeToOn;
        /// <summary>
        /// Чувствительность вращения камеры мышкой.
        /// </summary>
        [SerializeField] private int m_CamSensetive;

        /// <summary>
        /// Множитель для Time.deltaTime для скорости движения камеры за кораблем.
        /// </summary>
        [Space][SerializeField][Min(0)] private float m_CameraMoveSpeedMult;
        /// <summary>
        /// Множитель для Time.deltaTime во всех вызовах метода Lerp.
        /// </summary>
        [SerializeField][Min(0)] private float m_CameraLerpRatio;

        /// <summary>
        /// Расстояние одного шага отдаления/приближения камеры.
        /// </summary>
        [Space][SerializeField][Min(0)] private int m_CamZoomStep = 5;
        /// <summary>
        /// Макс. количество шагов отдаления/приближения камеры.
        /// </summary>
        [SerializeField][Min(0)] private int m_CamZoomStepsMaxAmount = 6;

        /// <summary>
        /// Максимальное отклонение от изначального положения камеры для её медленного рандомного движения. (когда корабль не движется)
        /// </summary>
        [Space][SerializeField][Min(0)] private float m_CamSlowShakeMaxAmplitude;
        /// <summary>
        /// Скорость медленного рандомного движения камеры.
        /// </summary>
        [SerializeField][Min(0)] private float m_CamSlowShakeSpeed;
        /// <summary>
        /// Новая позиция, к которой будет двигаться камера при медленном рандомном движении.
        /// </summary>
        [SerializeField][Min(0)] private float m_CamSlowShakeNewPosTime;

        /// <summary>
        /// Величина, на которую камера поднимается вверх над кораблем при следовании за целью.
        /// </summary>
        [Space][SerializeField] private float m_CamFollowTargetOffsetZ;

        private SpaceShip m_TargetShip;
        public void Construct(SpaceShip obj) => m_TargetShip = obj;

        private UITargetSelection m_TargetSelection;
        public void Construct(UITargetSelection obj) => m_TargetSelection = obj;

        private Camera m_Camera;

        /// <summary>
        /// Стартовая позиция camTargetParent.
        /// </summary>
        private Vector3 m_CamTargetParentStartLocalPos;
        /// <summary>
        /// Стартовая позиция camTarget.
        /// </summary>
        private Vector3 m_CamTargetStartLocalPos;

        /// <summary>
        /// Следует ли камера за кораблем. 
        /// </summary>
        private bool m_CamFollowShip;
        /// <summary>
        /// Следует ли камера за выделенной целью.
        /// </summary>
        private bool m_CamFollowTarget;

        /// <summary>
        /// Переменная, хранящая поворот камеры мышкой по оси X.
        /// </summary>
        private float m_CamTargetRotationX;
        /// <summary>
        /// Переменная, хранящая поворот камеры мышкой по оси Y.
        /// </summary>
        private float m_CamTargetRotationY;

        /// <summary>
        /// Переменная, хранящая расстояние, на которое двигается камера при приближении/отдалении по оси Z.
        /// </summary>
        private float m_CamZoom;
        /// <summary>
        /// Пороговое значение остановки методов Lerp для движения камеры.
        /// </summary>
        private const float CAMERA_MOVE_THRESHOLD = 0.001f;

        /// <summary>
        /// Позиция рандомной точки, за которой следует камера для эффекта медленного рандомного движения.
        /// </summary>
        private Vector3 m_CamSlowShakePos;
        /// <summary>
        /// Следующая рандомная позиция точки, за которой следует камера для эффекта медленного рандомного движения.
        /// </summary>
        private Vector3 m_CamSlowShakeNewPos;
        /// <summary>
        /// Таймер обновления следующей рандомной точки для эффекта медленного рандомного движения.
        /// </summary>
        private float m_CamSlowShakeNewPosTimer;
        /// <summary>
        /// Таймер для времени, через которое включается режим камеры после бездействия движения мышки.
        /// </summary>
        private float m_CameraModeOnTimer;

        #endregion

        #region Unity Events
        private void Awake()
        {
            m_Camera = GetComponent<Camera>();

            switch (m_CameraMode)
            {
                case CameraMode.FollowShip:
                    FollowShipOn();
                    break;
                case CameraMode.FollowTarget:
                    FollowTargetOn();
                    break;
                case CameraMode.Free:
                    FreeCameraOn();
                    break;
                default:
                    FollowShipOn();
                    break;
            }

            m_CamTargetParentStartLocalPos = m_CamTargetParent.localPosition;
            m_CamTargetStartLocalPos = m_CamTarget.localPosition;

            m_CamSlowShakePos = m_CamTargetParentStartLocalPos;
            CamSlowShakeRandomizeNextPosition();
        }

        private void Update()
        {
            if (m_Camera == null || m_CamTarget == null) return;

            TempSwitchCameraMode();   
        }

        private void FixedUpdate()
        {
            CameraMove();
        }

        #endregion

        #region Private API
        //ПЕРЕКЛЮЧЕНИЕ РЕЖИМОВ КНОПКАМИ, ВРЕМЕННО!!!
        private void TempSwitchCameraMode()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                m_CameraMode = CameraMode.FollowShip;

            if (Input.GetKeyDown(KeyCode.Alpha2))
                m_CameraMode = CameraMode.FollowTarget;

            if (Input.GetKeyDown(KeyCode.Alpha3))
                m_CameraMode = CameraMode.Free;
        }

        /// <summary>
        /// Всё движение камеры.
        /// </summary>
        private void CameraMove()
        {
            if (m_CamFollowTarget == false && m_CamFollowShip == false)
                AlignOnZAxis();

            if (Input.GetMouseButton(0))
            {
                m_CameraModeOnTimer = 0;

                FreeCameraOn();

                MouseRotation();
            }

            // Если корабль уничтожен, то остается только движение камеры мышкой.
            if (m_TargetShip != null)
            {
                m_CameraModeOnTimer += Time.deltaTime;

                CameraZoom();

                if (m_TargetShip.LinearVelocity == 0)
                    CamSmoothShake();
                else
                    m_CamTargetParent.localPosition = LerpVectorWithThreshold(m_CamTargetParent.localPosition, Vector3.zero, Time.deltaTime, CAMERA_MOVE_THRESHOLD);

                if (m_CameraModeOnTimer >= m_CameraModeTimeToOn)
                    CameraModeOn();

                if (m_CamFollowShip)
                    CamFollow();

                if (m_CamFollowTarget)
                    CamFollowTarget();
                else
                    CamCancelFollowTarget();

                m_CamTargetRoot.position = Vector3.Lerp(m_CamTargetRoot.position, m_TargetShip.transform.position, m_TargetShip.MaxLinearVelocity * m_CameraMoveSpeedMult * Time.deltaTime);
            }

            m_Camera.transform.SetPositionAndRotation(m_CamTarget.position, m_CamTarget.rotation);
        }

        /// <summary>
        /// Вращение мышкой
        /// </summary>
        private void MouseRotation()
        {
            if (m_CamTargetRotationY > 360) m_CamTargetRotationY -= 360;
            else if (m_CamTargetRotationY < -360) m_CamTargetRotationY += 360;

            m_CamTargetRotationX = Mathf.LerpAngle(m_CamTargetRotationX, m_CamTargetParent.localEulerAngles.x, 1);
            m_CamTargetRotationY = Mathf.LerpAngle(m_CamTargetRotationY, m_CamTargetParent.localEulerAngles.y, 1);

            float mouseX = Input.GetAxis("Mouse X") * m_CamSensetive * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * m_CamSensetive * Time.deltaTime;

            m_CamTargetRotationX -= mouseY;
            m_CamTargetRotationY += mouseX;
            m_CamTargetRotationX = Mathf.Clamp(m_CamTargetRotationX, -90f, 90f);

            m_CamTargetParent.localRotation = Quaternion.Euler(m_CamTargetRotationX, m_CamTargetRotationY, m_CamTargetParent.localEulerAngles.z);
        }

        /// <summary>
        /// Метод переключает переменные режима камеры в зависимости от текущего режима камеры.
        /// </summary>
        private void CameraModeOn()
        {
            if (m_CameraMode == CameraMode.Free)
                FreeCameraOn();

            if (m_CameraMode == CameraMode.FollowShip)
                FollowShipOn();

            if (m_CameraMode == CameraMode.FollowTarget)
            {
                if (m_TargetSelection.CurrentTarget != null)
                    FollowTargetOn();
                else
                    FollowShipOn();
            }
        }

        /// <summary>
        /// Включает переменную режима следования за кораблем.
        /// </summary>
        private void FollowShipOn()
        {
            m_CamFollowTarget = false;
            m_CamFollowShip = true;
        }
        /// <summary>
        /// Включает переменную режим следования за целью.
        /// </summary>
        private void FollowTargetOn()
        {
            m_CamFollowShip = false;
            m_CamFollowTarget = true;
        }
        /// <summary>
        /// Выключает все переменные режимов для режима свободной камеры.
        /// </summary>
        private void FreeCameraOn()
        {
            m_CamFollowShip = false;
            m_CamFollowTarget = false;
        }

        /// <summary>
        /// Доворот по оси Z для выравнивания корабля. (работает только тогда, когда камера не занята другими методами управления)
        /// </summary>
        private void AlignOnZAxis()
        {
            float camRotationZ = m_CamTargetParent.localEulerAngles.z;
            camRotationZ = Mathf.LerpAngle(camRotationZ, 0, Time.deltaTime);
            m_CamTargetParent.localRotation = Quaternion.Euler(m_CamTargetParent.localEulerAngles.x, m_CamTargetParent.localEulerAngles.y, camRotationZ);
        }

        /// <summary>
        /// Отдаление/приближение камеры.
        /// </summary>
        void CameraZoom()
        {
            m_CamZoom = Mathf.Clamp(m_CamZoom + Input.GetAxis("Mouse ScrollWheel") * m_CamZoomStep, -(m_CamZoomStep * m_CamZoomStepsMaxAmount) * 0.1f, 0f);

            Vector3 target = new Vector3(m_CamTarget.localPosition.x, m_CamTarget.localPosition.y, m_CamTargetStartLocalPos.z + m_CamZoom);
            m_CamTarget.localPosition = LerpVectorWithThreshold(m_CamTarget.localPosition, target, m_CameraLerpRatio * Time.deltaTime, CAMERA_MOVE_THRESHOLD);
        }

        /// <summary>
        /// Следование камеры за кораблем.
        /// </summary>
        private void CamFollow()
        {
            SetCamTargetRootRotation(m_TargetShip.transform.rotation);

            m_CamTargetParent.rotation = Quaternion.Slerp(m_CamTargetParent.rotation, m_TargetShip.transform.rotation, m_CameraLerpRatio * Time.deltaTime);
        }

        /// <summary>
        /// Метод жестко задает вращение CamTargetRoot по заданному вращению, при этом оставляя вращение camTargetParent в том же вращении.
        /// </summary>
        /// <param name="targetRotation">Целевое вращение.</param>
        void SetCamTargetRootRotation(Quaternion targetRotation)
        {
            Quaternion q = m_CamTargetParent.rotation;
            m_CamTargetRoot.rotation = targetRotation;
            m_CamTargetParent.rotation = q;
        }

        /// <summary>
        /// Следование камеры за целью.
        /// </summary>
        private void CamFollowTarget()
        {
            Vector3 dir = m_TargetSelection.CurrentTarget.transform.position - m_CamTargetParent.position;

            SetCamTargetRootRotation(Quaternion.LookRotation(dir));

            m_CamTargetParent.rotation = Quaternion.Slerp(m_CamTargetParent.rotation, Quaternion.LookRotation(dir), m_CameraLerpRatio * Time.deltaTime);

            Vector3 target = new Vector3(m_CamTarget.localPosition.x, m_CamTargetStartLocalPos.y + m_CamFollowTargetOffsetZ, m_CamTarget.localPosition.z);
            m_CamTarget.localPosition = LerpVectorWithThreshold(m_CamTarget.localPosition, target, m_CameraLerpRatio * Time.deltaTime, CAMERA_MOVE_THRESHOLD);
        }

        /// <summary>
        /// Возвращение камеры в исходную позицию после прекращения следования за целью.
        /// </summary>
        void CamCancelFollowTarget()
        {
            Vector3 target = new Vector3(m_CamTarget.localPosition.x, m_CamTargetStartLocalPos.y, m_CamTarget.localPosition.z);
            m_CamTarget.localPosition = LerpVectorWithThreshold(m_CamTarget.localPosition, target, m_CameraLerpRatio * Time.deltaTime, CAMERA_MOVE_THRESHOLD);
        }

        ///<summary>
        /// Линейная интерполяция вектора до целевого с пороговым значением.
        /// </summary>
        /// <param name="a">Начальный вектор.</param>
        /// <param name="b">Целевой вектор.</param>
        /// <param name="ratio">Interpolation ratio.</param>
        /// <param name="threshold">Пороговое значение.</param>
        /// <returns>Интерполированное значение или целевой вектор по пороговому значению.</returns>
        private Vector3 LerpVectorWithThreshold(Vector3 a, Vector3 b, float ratio, float threshold)
        {
            if (a == b) return b;

            if (Mathf.Abs(Mathf.Abs(a.x) - Mathf.Abs(b.x)) <= threshold &&
                Mathf.Abs(Mathf.Abs(a.y) - Mathf.Abs(b.y)) <= threshold &&
                Mathf.Abs(Mathf.Abs(a.z) - Mathf.Abs(b.z)) <= threshold)
                return b;

            a = Vector3.Lerp(a, b, ratio);

            return a;
        }

        /// <summary>
        /// Медленное рандомное движение камеры, когда корабль стоит на месте
        /// </summary>
        void CamSmoothShake()
        {
            m_CamSlowShakeNewPosTimer += Time.deltaTime;

            m_CamSlowShakePos = Vector3.Slerp(m_CamSlowShakePos, m_CamSlowShakeNewPos, Time.deltaTime);

            m_CamTargetParent.localPosition = Vector3.MoveTowards(m_CamTargetParent.localPosition, m_CamSlowShakePos, Time.deltaTime * m_CamSlowShakeSpeed);

            if (m_CamSlowShakeNewPosTimer >= m_CamSlowShakeNewPosTime)
                CamSlowShakeRandomizeNextPosition();
        }

        /// <summary>
        /// Получение рандомной позиции новой точки для медленного рандомного движения камеры.
        /// </summary>
        private void CamSlowShakeRandomizeNextPosition()
        {
            m_CamSlowShakeNewPos = Random.onUnitSphere * m_CamSlowShakeMaxAmplitude;
            m_CamSlowShakeNewPosTimer = 0;
        }
        #endregion
    }
}