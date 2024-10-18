using UnityEngine;
using System.IO.Ports;
using TMPro;
using System;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;


enum TaskState
{
    INIT,
    WAIT_COMMANDS
}

public class config : MonoBehaviour
{
    private static TaskState taskState = TaskState.INIT;
    private SerialPort _serialPort;
    private byte[] buffer;
    public TextMeshProUGUI myText;
    public TextMeshProUGUI Temper;
    public TextMeshProUGUI Inunda;

    private float counter = 300.0f;
    private int numTemp = 1;
    private float numInun = 1.0f;

    bool inicio;
    private bool ackReceived = false; // Para rastrear si se ha recibido el ACK de Arduino

    void Start()
    {
        _serialPort = new SerialPort();
        _serialPort.PortName = "COM5";
        _serialPort.BaudRate = 115200;
        _serialPort.DtrEnable = true;
        _serialPort.NewLine = "\n";
        _serialPort.Open();
        Debug.Log("Open Serial Port");
        buffer = new byte[128];
    }

    void Update()
    {
        if (numInun == -4) numInun = 1;
        if (numTemp == -4) numTemp = 1;
        if (numInun >= 4) numInun = 1;
        if (numTemp >= 4) numTemp = 1;

        myText.text = counter != -5 ? counter.ToString() : "Desactivado";
        Inunda.text = numInun != -5 ? numInun.ToString() : "Desactivado";
        Temper.text = numTemp != -5 ? "Activado" : "Desactivado";

        switch (taskState)
        {
            case TaskState.INIT:
                taskState = TaskState.WAIT_COMMANDS;
                _serialPort.Write("reset\n");
                Debug.Log("reset");
                Debug.Log("WAIT COMMANDS");
                break;

            case TaskState.WAIT_COMMANDS:
                if (Input.GetKeyDown(KeyCode.S) && counter < 500)
                {
                    counter++;
                }
                if (Input.GetKeyDown(KeyCode.B) && counter > 100)
                {
                    counter--;
                }

                // Cuando se presiona la tecla L, envía los datos al microcontrolador
                if (inicio)
                {
                    SendData(counter, numInun, numTemp);
                    SceneManager.LoadScene("Preguntas");
                }


                break;

            default:
                Debug.Log("State Error");
                break;
        }
    }

    // Método para enviar los datos en formato little-endian
    void SendData(float counter, float numInun, float numTemp)
    {
        // Calcular el checksum como la suma de los otros tres valores
        float checksum = counter + numInun + numTemp;

        // Crear un array de bytes para almacenar los datos (16 bytes ahora)
        byte[] dataToSend = new byte[16];

        // Convertir cada valor a su representación en bytes (little-endian)
        byte[] counterBytes = System.BitConverter.GetBytes(counter);
        byte[] numInunBytes = System.BitConverter.GetBytes(numInun);
        byte[] numTempBytes = System.BitConverter.GetBytes(numTemp);
        byte[] checksumBytes = System.BitConverter.GetBytes(checksum);

        // Copiar los bytes en el array principal
        System.Buffer.BlockCopy(counterBytes, 0, dataToSend, 0, 4);
        System.Buffer.BlockCopy(numInunBytes, 0, dataToSend, 4, 4);
        System.Buffer.BlockCopy(numTempBytes, 0, dataToSend, 8, 4);
        System.Buffer.BlockCopy(checksumBytes, 0, dataToSend, 12, 4);

        // Enviar los bytes al puerto serial
        _serialPort.Write(dataToSend, 0, dataToSend.Length);

        // Mensaje de depuración
        Debug.Log("Data sent: " + BitConverter.ToString(dataToSend));
    }

    public void aumentar() { if (counter != -5 && counter < 350) counter += 0.5f; else counter = 350.0f; }
    public void disminuir() { if (counter != -5 && counter > 100) counter--; else counter = 100.0f; }
    public void maximo() { counter = 350.0f; }
    public void iniciar() { inicio = true; }
    public void minimo() { counter = 100; }
    public void temperatura() { numTemp++; }
    public void inundacion() { numInun = numInun + 0.5f; }
    public void desactCounter() { counter = -5; }
    public void desactTemp() { numTemp = -5; }
    public void desactInun() { numInun = -5; }

    void OnDestroy() { if (_serialPort != null && _serialPort.IsOpen) { _serialPort.Close(); Debug.Log("Serial port closed in OnDestroy."); } }
    void OnApplicationQuit() { if (_serialPort != null && _serialPort.IsOpen) { _serialPort.Close(); Debug.Log("Serial port closed in OnApplicationQuit."); } }
}
