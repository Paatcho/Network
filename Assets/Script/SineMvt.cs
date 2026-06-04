using UnityEngine;
using Unity.Mathematics;

public class SineMvt : MonoBehaviour
{

    public GameObject movingAsset;
    private Vector3 initPos;
    private Vector3 sinusWave;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Vector3 initPos = transform.localPosition;
	Vector3 sinusWave = new Vector3(0.0f, 0.0f, 0.0f);
	if (movingAsset == null)    {
		initPos = transform.position;
	}
	else {

		initPos = movingAsset.transform.position;
	}


    }

    // Update is called once per frame
    void Update()
    {
	sinusWave = new Vector3(0.0f, 0.0f, Mathf.Sin(Time.time));
        transform.position = new Vector3((initPos.x + sinusWave.x), (initPos.y + sinusWave.y), (initPos.z + sinusWave.z));
    }
}