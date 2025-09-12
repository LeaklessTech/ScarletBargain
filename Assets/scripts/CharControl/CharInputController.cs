using UnityEngine;

public class CharInputController : MonoBehaviour
{
    private float filteredForwardInput = 0f;
    private float filteredTurnInput = 0f;
    private float forwardSpeedLimit = 1f;

    public bool InputMapToCircular = true;
    public float forwardInputFilter = 1f;
    public float turnInputFilter = 1f;

    public float Forward
    {
        get;
        private set;
    }

    public float Turn
    {
        get;
        private set;
    }

    public bool Jump
    {
        get;
        private set;
    }

    void Update()
    {

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");


        if (InputMapToCircular)
        {
            // circular h, v
            h = h * Mathf.Sqrt(1f - 0.5f * v * v);
            v = v * Mathf.Sqrt(1f - 0.5f * h * h);
        }


        filteredForwardInput = Mathf.Clamp(Mathf.Lerp(filteredForwardInput, v,
            Time.deltaTime * forwardInputFilter), -forwardSpeedLimit, forwardSpeedLimit);

        filteredTurnInput = Mathf.Lerp(filteredTurnInput, h,
            Time.deltaTime * turnInputFilter);

        Forward = filteredForwardInput;
        Turn = filteredTurnInput;

        Jump = Input.GetButtonDown("Jump");

    }
}
