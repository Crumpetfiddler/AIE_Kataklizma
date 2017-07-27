﻿// Patrol.cs
using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Patrol : MonoBehaviour
{

    public Transform[] points;
    private int destPoint = 0;
    private NavMeshAgent agent;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        agent.autoBraking = false;

        GotoNextPoint();
    }


    void GotoNextPoint()
    {
        // Do nothing if the array is empty
        if (points.Length == 0)
            return;


        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = Random.Range(0, points.Length);
        Debug.Log(destPoint);


        // Goto the selected destination
        agent.destination = points[destPoint].position;

        // Pull random element from array:
        //var selected = points[Random.Range(0, points.Length)];
    }


    void Update()
    {
        // Choose the next destination point when the agent gets
        // close to the current one.
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GotoNextPoint();
    }
}