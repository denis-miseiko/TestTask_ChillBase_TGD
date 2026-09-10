using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class InOutCar : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] CinemachineCamera myCamera = null;
    [SerializeField] GameObject playerCameraPoint = null;
    [SerializeField] GameObject carCameraPoint = null;


    [Space, Header("Player")]
    [SerializeField] GameObject player = null;
    [SerializeField] float closeDistance = 5f;

    [Space, Header("Car")]
    [SerializeField] GameObject car = null;
    [SerializeField] PrometeoCarController carController = null;

    [Space, Header("Input")]
    [SerializeField] KeyCode enterExitKey = KeyCode.E;

    bool inCar = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        carController.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(enterExitKey))
        {
            if (inCar)
                GetOutOfCar();
            else if (Vector3.Distance(car.transform.position, player.transform.position) < closeDistance)
                GetIntoCar();
        }
    }

    void GetOutOfCar()
    {
        inCar = false;

        carController.enabled = false;

        player.transform.position = car.transform.position + car.transform.TransformDirection(Vector3.left);
        SimulateWPress();
        player.SetActive(true);

        myCamera.Follow = playerCameraPoint.transform;
    }

    void GetIntoCar()
    {
        inCar = true;

        carController.enabled = true;

        myCamera.Follow = carCameraPoint.transform;
        player.SetActive(false);
    }

    void SimulateWPress()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Нажимаем W
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));

        // Отпускаем W (передаём пустое состояние)
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
    }
}
