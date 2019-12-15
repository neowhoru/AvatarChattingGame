using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    Vector3 targetPosition;
    Vector3 lookAtTarget;
    Quaternion playerRot;
    float rotSpeed = 5;
    float speed = 10;
    public Vector3 lastPosition;

    public string playerName = "XXX";

    public TMPro.TMP_Text playerText;
    // Start is called before the first frame update
    private Animator anim;
    void Start()
    {
        lastPosition = transform.position;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void MoveToPosition(float xPos, float yPos, float zPos, float yRot)
    {
        targetPosition = new Vector3(xPos,yPos,zPos);
        playerRot = new Quaternion(playerRot.x, yRot, playerRot.z, playerRot.w);
        if (transform.position != targetPosition)
        {
            anim.SetBool("param_idletorunning", true);
        }
        else
        {
            anim.SetBool("param_idletorunning", false);
        }
        lookAtTarget = new Vector3(targetPosition.x - transform.position.x, transform.position.y, targetPosition.z - transform.position.z);
        playerRot = Quaternion.LookRotation(lookAtTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, playerRot, rotSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
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
