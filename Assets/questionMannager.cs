using UnityEngine;
using System.IO.Ports;
using TMPro;
using UnityEngine.SceneManagement;

public class questionMannager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip soundClip1;
    public AudioClip soundClip2;

    private SerialPort _serialPort;
    public TextMeshProUGUI counterText; // Para mostrar el contador
    public TextMeshProUGUI tempText;    // Para mostrar la temperatura real
    public TextMeshProUGUI inunText;    // Para mostrar la inundación

    public TextMeshProUGUI preguntaText;
    public GameObject inunda2;
    public GameObject inunda1;
    public GameObject temp2;
    public GameObject temp1;

    public GameObject[] QObjects = new GameObject[15];

    private float counter = 0;
    private float numInun = 0;
    private float realTemp = 0.0f;
    private float actualInun = -1;   // Iniciamos con un valor inválido
    private int displayInun = 100;   // Valor que se mostrará según el switch

    int contador = 0;

    int c = 0;
    int c2 = 0;

    void Start()
    {
        // Configuración del puerto serie
        _serialPort = new SerialPort();
        _serialPort.PortName = "COM5";
        _serialPort.BaudRate = 115200;
        _serialPort.DtrEnable = true;
        _serialPort.NewLine = "\n";
        _serialPort.Open();
        Debug.Log("Open Serial Port");
    }

    void Update()
    {
        preguntaText.text = contador.ToString() + "/15";

        if (contador >= 1 && contador <= 15)
        {
            QObjects[contador - 1].SetActive(false);
        }
        if (contador >= 15)
        {
            SceneManager.LoadScene("Victoria");
        }

        // Leer datos desde el microcontrolador
        if (_serialPort.BytesToRead > 0)
        {
            string response = _serialPort.ReadLine(); // Leer la línea enviada desde Arduino


            if (!string.IsNullOrEmpty(response))
            {
                // Dividir la respuesta por comas
                string[] values = response.Split(',');

                // Asegurarse de que se recibieron los valores correctos
                if (values.Length == 3)
                {
                    if (float.TryParse(values[0], out float receivedCounter))
                    {
                        counter = receivedCounter / 100.0f;
                    }

                    if (float.TryParse(values[1], out float receivedInun))
                    {
                        if (actualInun == -1 && receivedInun > 0)
                        {
                            actualInun = receivedInun / 100.0f;

                        }
                        numInun = receivedInun;
                    }

                    if (float.TryParse(values[2], out float receivedTemp))
                    {
                        realTemp = receivedTemp / 100.0f;
                        c2++;
                    }

                    // Actualizar UI
                    counterText.text = counter == -5 ? "Contador: Desactivado" : "Contador: " + counter.ToString();

                    if (actualInun != -1)
                    {
                        switch (actualInun)
                        {
                            case 1:
                                inunda1.SetActive(true);
                                inunda2.SetActive(false);
                                break;
                            case 2:
                                inunda2.SetActive(true);
                                inunda1.SetActive(false);
                                break;
                            case 3:
                                inunda1.SetActive(false);
                                inunda2.SetActive(false);
                                break;
                            case 0:
                                SceneManager.LoadScene("GameOver");
                                break;
                        }
                        inunText.text = "Inundación: " + actualInun.ToString() + "%";
                    }

                    tempText.text = realTemp == -5 ? "Temperatura: Desactivado" : "Temperatura: " + realTemp.ToString("F2") + "°";
                }
            }
        }

        if (c == 0 && c2 >= 1)
        {
            float checksum = counter + numInun + realTemp;
            Debug.Log("Checksum recibido: " + checksum);
            Debug.Log("Checksum calculado: " + checksum);
            Debug.Log("Checksum validado correctamente.");
            c++;
        }
    }
    public void OnMistakeButtonClick()
    {
        if (actualInun > 0)
        {
            actualInun = actualInun - 0.5f;
        }

        audioSource.clip = soundClip1;
        audioSource.Play();

        Debug.Log("Mistake " + actualInun);
    }

    public void Correcto()
    {
        Debug.Log("Good " + contador);
        contador++;

        audioSource.clip = soundClip2;
        audioSource.Play();
    }
    void OnDestroy()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            Debug.Log("Serial port closed in OnDestroy.");
        }
    }

    void OnApplicationQuit()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            Debug.Log("Serial port closed in OnApplicationQuit.");
        }
    }
}