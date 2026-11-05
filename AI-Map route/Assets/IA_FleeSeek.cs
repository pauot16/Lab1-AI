using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine;

public class IA_FleeSeek : MonoBehaviour
{

private NavMeshAgent ai;

[SerializeField] private GameObject target;

public bool setFlee = false;

public bool setSeek = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Update()
    {
        if(Keyboard.current.f1Key.wasPressedThisFrame)
        {
            setFlee = !setFlee;
            setSeek = !setSeek;
            print($"Flee:{setFlee}\nSeek: {setSeek}");
        }
       if(setFlee) Seek(target.transform.position); 
       else Flee(target.transform.position);
    }

    private void Start()
    {
        setFlee = true;
    }

    // Update is called once per frame
    void Awake()
    {
        ai = GetComponent<NavMeshAgent>();
    }

    private void Seek(Vector3 location)
    {
        ai.SetDestination(location);
    }

    private void Flee(Vector3 location)
    {
        Vector3 fleeVector = location - transform.position;
        ai.SetDestination(transform.position - fleeVector);
    }
}
