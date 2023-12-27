using UnityEngine;

namespace UnknownSpace
{
    [RequireComponent (typeof (Camera))]
    public class CameraController : MonoBehaviour, IDependency<SpaceShip>,IDependency<UITargetSelection>
    {
        #region Properties
        /// <summary>
        /// �������� ������ ������ ������. (����� ��� ����������� �������� ������ �� ��� Z)
        /// </summary>
        [SerializeField] private Transform m_CamTargetRoot;
        /// <summary>
        /// �������� ������, ����� ���������� �������� � camTargetRoot. (����� ��� ����������� �������� ������ ������ �������)
        /// </summary>
        [SerializeField] private Transform m_CamTargetParent;
        /// <summary>
        /// ����� ������������ ����� ������.
        /// </summary>
        [SerializeField] private Transform m_CamTarget;
        /// <summary>
        /// ������ �� �����, ����� ������� �������� ����������, �������� ������� ���������� ����.
        /// </summary>

        /// <summary>
        /// ������������� ����� ������. (��������� ������, ���������� �� ��������, ���������� �� �����)
        /// </summary>
        [Space(20)] public CameraMode m_CameraMode;
        /// <summary>
        /// �����, ����� ������� ���������� ����� ������ ����� ����������� �������� �����.
        /// </summary>
        [SerializeField][Min(0)] private float m_CameraModeTimeToOn;
        /// <summary>
        /// ���������������� �������� ������ ������.
        /// </summary>
        [SerializeField] private int m_CamSensetive;

        /// <summary>
        /// ��������� ��� Time.deltaTime ��� �������� �������� ������ �� ��������.
        /// </summary>
        [Space][SerializeField][Min(0)] private float m_CameraMoveSpeedMult;
        /// <summary>
        /// ��������� ��� Time.deltaTime �� ���� ������� ������ Lerp.
        /// </summary>
        [SerializeField][Min(0)] private float m_CameraLerpRatio;

        /// <summary>
        /// ���������� ������ ���� ���������/����������� ������.
        /// </summary>
        [Space][SerializeField][Min(0)] private int m_CamZoomStep = 5;
        /// <summary>
        /// ����. ���������� ����� ���������/����������� ������.
        /// </summary>
        [SerializeField][Min(0)] private int m_CamZoomStepsMaxAmount = 6;

        /// <summary>
        /// ������������ ���������� �� ������������ ��������� ������ ��� � ���������� ���������� ��������. (����� ������� �� ��������)
        /// </summary>
        [Space][SerializeField][Min(0)] private float m_CamSlowShakeMaxAmplitude;
        /// <summary>
        /// �������� ���������� ���������� �������� ������.
        /// </summary>
        [SerializeField][Min(0)] private float m_CamSlowShakeSpeed;
        /// <summary>
        /// ����� �������, � ������� ����� ��������� ������ ��� ��������� ��������� ��������.
        /// </summary>
        [SerializeField][Min(0)] private float m_CamSlowShakeNewPosTime;

        /// <summary>
        /// ��������, �� ������� ������ ����������� ����� ��� �������� ��� ���������� �� �����.
        /// </summary>
        [Space][SerializeField] private float m_CamFollowTargetOffsetZ;

        private SpaceShip m_TargetShip;
        public void Construct(SpaceShip obj) => m_TargetShip = obj;

        private UITargetSelection m_TargetSelection;
        public void Construct(UITargetSelection obj) => m_TargetSelection = obj;

        private Camera m_Camera;

        /// <summary>
        /// ��������� ������� camTargetParent.
        /// </summary>
        private Vector3 m_CamTargetParentStartLocalPos;
        /// <summary>
        /// ��������� ������� camTarget.
        /// </summary>
        private Vector3 m_CamTargetStartLocalPos;

        /// <summary>
        /// ������� �� ������ �� ��������. 
        /// </summary>
        private bool m_CamFollowShip;
        /// <summary>
        /// ������� �� ������ �� ���������� �����.
        /// </summary>
        private bool m_CamFollowTarget;

        /// <summary>
        /// ����������, �������� ������� ������ ������ �� ��� X.
        /// </summary>
        private float m_CamTargetRotationX;
        /// <summary>
        /// ����������, �������� ������� ������ ������ �� ��� Y.
        /// </summary>
        private float m_CamTargetRotationY;

        /// <summary>
        /// ����������, �������� ����������, �� ������� ��������� ������ ��� �����������/��������� �� ��� Z.
        /// </summary>
        private float m_CamZoom;
        /// <summary>
        /// ��������� �������� ��������� ������� Lerp ��� �������� ������.
        /// </summary>
        private const float CAMERA_MOVE_THRESHOLD = 0.001f;

        /// <summary>
        /// ������� ��������� �����, �� ������� ������� ������ ��� ������� ���������� ���������� ��������.
        /// </summary>
        private Vector3 m_CamSlowShakePos;
        /// <summary>
        /// ��������� ��������� ������� �����, �� ������� ������� ������ ��� ������� ���������� ���������� ��������.
        /// </summary>
        private Vector3 m_CamSlowShakeNewPos;
        /// <summary>
        /// ������ ���������� ��������� ��������� ����� ��� ������� ���������� ���������� ��������.
        /// </summary>
        private float m_CamSlowShakeNewPosTimer;
        /// <summary>
        /// ������ ��� �������, ����� ������� ���������� ����� ������ ����� ����������� �������� �����.
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
        //������������ ������� ��������, ��������!!!
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
        /// �� �������� ������.
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

            // ���� ������� ���������, �� �������� ������ �������� ������ ������.
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
        /// �������� ������
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
        /// ����� ����������� ���������� ������ ������ � ����������� �� �������� ������ ������.
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
        /// �������� ���������� ������ ���������� �� ��������.
        /// </summary>
        private void FollowShipOn()
        {
            m_CamFollowTarget = false;
            m_CamFollowShip = true;
        }
        /// <summary>
        /// �������� ���������� ����� ���������� �� �����.
        /// </summary>
        private void FollowTargetOn()
        {
            m_CamFollowShip = false;
            m_CamFollowTarget = true;
        }
        /// <summary>
        /// ��������� ��� ���������� ������� ��� ������ ��������� ������.
        /// </summary>
        private void FreeCameraOn()
        {
            m_CamFollowShip = false;
            m_CamFollowTarget = false;
        }

        /// <summary>
        /// ������� �� ��� Z ��� ������������ �������. (�������� ������ �����, ����� ������ �� ������ ������� �������� ����������)
        /// </summary>
        private void AlignOnZAxis()
        {
            float camRotationZ = m_CamTargetParent.localEulerAngles.z;
            camRotationZ = Mathf.LerpAngle(camRotationZ, 0, Time.deltaTime);
            m_CamTargetParent.localRotation = Quaternion.Euler(m_CamTargetParent.localEulerAngles.x, m_CamTargetParent.localEulerAngles.y, camRotationZ);
        }

        /// <summary>
        /// ���������/����������� ������.
        /// </summary>
        void CameraZoom()
        {
            m_CamZoom = Mathf.Clamp(m_CamZoom + Input.GetAxis("Mouse ScrollWheel") * m_CamZoomStep, -(m_CamZoomStep * m_CamZoomStepsMaxAmount) * 0.1f, 0f);

            Vector3 target = new Vector3(m_CamTarget.localPosition.x, m_CamTarget.localPosition.y, m_CamTargetStartLocalPos.z + m_CamZoom);
            m_CamTarget.localPosition = LerpVectorWithThreshold(m_CamTarget.localPosition, target, m_CameraLerpRatio * Time.deltaTime, CAMERA_MOVE_THRESHOLD);
        }

        /// <summary>
        /// ���������� ������ �� ��������.
        /// </summary>
        private void CamFollow()
        {
            SetCamTargetRootRotation(m_TargetShip.transform.rotation);

            m_CamTargetParent.rotation = Quaternion.Slerp(m_CamTargetParent.rotation, m_TargetShip.transform.rotation, m_CameraLerpRatio * Time.deltaTime);
        }

        /// <summary>
        /// ����� ������ ������ �������� CamTargetRoot �� ��������� ��������, ��� ���� �������� �������� camTargetParent � ��� �� ��������.
        /// </summary>
        /// <param name="targetRotation">������� ��������.</param>
        void SetCamTargetRootRotation(Quaternion targetRotation)
        {
            Quaternion q = m_CamTargetParent.rotation;
            m_CamTargetRoot.rotation = targetRotation;
            m_CamTargetParent.rotation = q;
        }

        /// <summary>
        /// ���������� ������ �� �����.
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
        /// ����������� ������ � �������� ������� ����� ����������� ���������� �� �����.
        /// </summary>
        void CamCancelFollowTarget()
        {
            Vector3 target = new Vector3(m_CamTarget.localPosition.x, m_CamTargetStartLocalPos.y, m_CamTarget.localPosition.z);
            m_CamTarget.localPosition = LerpVectorWithThreshold(m_CamTarget.localPosition, target, m_CameraLerpRatio * Time.deltaTime, CAMERA_MOVE_THRESHOLD);
        }

        ///<summary>
        /// �������� ������������ ������� �� �������� � ��������� ���������.
        /// </summary>
        /// <param name="a">��������� ������.</param>
        /// <param name="b">������� ������.</param>
        /// <param name="ratio">Interpolation ratio.</param>
        /// <param name="threshold">��������� ��������.</param>
        /// <returns>����������������� �������� ��� ������� ������ �� ���������� ��������.</returns>
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
        /// ��������� ��������� �������� ������, ����� ������� ����� �� �����
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
        /// ��������� ��������� ������� ����� ����� ��� ���������� ���������� �������� ������.
        /// </summary>
        private void CamSlowShakeRandomizeNextPosition()
        {
            m_CamSlowShakeNewPos = Random.onUnitSphere * m_CamSlowShakeMaxAmplitude;
            m_CamSlowShakeNewPosTimer = 0;
        }
        #endregion
    }
}