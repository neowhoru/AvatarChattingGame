using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Vector3 targetPosition;
    Vector3 lookAtTarget;
    Quaternion playerRot;
    float rotSpeed = 5;
    float speed = 10;

    private Animator anim;
    private NetworkGameHandler networkGameHandler;

    public string playerName = "XXX";

    public TMPro.TMP_Text playerText;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        networkGameHandler = GetComponent<NetworkGameHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SetTargetPosition();
        }
        Move();
    }

    // https://www.youtube.com/watch?v=MAbei7eMlXg
    public void SetTargetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 1000))
        {
            targetPosition = hit.point;
            targetPosition.y = transform.position.y;
            //this.transform.LookAt(targetPosition);
            lookAtTarget = new Vector3(targetPosition.x - transform.position.x, transform.position.y, targetPosition.z - transform.position.z);
            playerRot = Quaternion.LookRotation(lookAtTarget);
            networkGameHandler.SendMoveMessage(targetPosition, playerRot.y);
        }
    }
    
    public void Move()
    {
        if (transform.position != targetPosition)
        {
            anim.SetBool("param_idletorunning", true);
        }
        else
        {
            anim.SetBool("param_idletorunning", false);
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, playerRot, rotSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
        playerText.SetText(playerName);
    }
}
