﻿using NUnit.Framework;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beatmap.Enums;
using Beatmap.Base;
using Beatmap.V2;
using Tests.Util;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class StrobeGeneratorTest
    {
        [UnityOneTimeSetUp]
        public IEnumerator LoadMap()
        {
            return TestUtils.LoadMapper();
        }

        [TearDown]
        public void ContainerCleanup()
        {
            BeatmapActionContainer.RemoveAllActionsOfType<BeatmapAction>();
            TestUtils.CleanupEvents();
        }

        public static void CheckEvent(BeatmapObjectContainerCollection container, int idx, float time, int type, int value, JSONNode customData = null)
        {
            IObject newObjA = container.LoadedObjects.Skip(idx).First();
            Assert.IsInstanceOf<IEvent>(newObjA);
            if (newObjA is IEvent newNoteA)
            {
                Assert.AreEqual(time, newNoteA.Time);
                Assert.AreEqual(type, newNoteA.Type);
                Assert.AreEqual(value, newNoteA.Value);

                // ToJSON causes gradient to get updated
                if (customData != null)
                {
                    Assert.AreEqual(customData.ToString(), newNoteA.ToJson()["_customData"].ToString());
                }
            }
        }

        [Test]
        public void ChromaStepGradient()
        {
            BeatmapObjectContainerCollection containerCollection = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Event);
            if (containerCollection is EventGridContainer eventsContainer)
            {
                Transform root = eventsContainer.transform.root;
                EventPlacement eventPlacement = root.GetComponentInChildren<EventPlacement>();

                IEvent eventA = new V2Event(2, (int)EventTypeValue.RingLights, (int)LightValue.RedOn, new JSONObject
                {
                    ["_color"] = new Color(0, 1, 0)
                });
                IEvent eventB = new V2Event(3, (int)EventTypeValue.RingLights, (int)LightValue.RedOn, new JSONObject
                {
                    ["_color"] = new Color(0, 0, 1)
                });
                IEvent eventC = new V2Event(3, (int)EventTypeValue.RingLights, (int)LightValue.RedOn, new JSONObject
                {
                    ["_lightID"] = 1,
                    ["_color"] = new Color(1, 0, 0)
                });

                foreach (IEvent IEvent in new IEvent[] { eventA, eventB, eventC })
                {
                    eventPlacement.queuedData = IEvent;
                    eventPlacement.queuedValue = eventPlacement.queuedData.Value;
                    eventPlacement.RoundedTime = eventPlacement.queuedData.Time;
                    eventPlacement.ApplyToMap();
                }

                SelectionController.Select(eventA);
                SelectionController.Select(eventB, true);
                // eventC is not selected

                StrobeGenerator strobeGenerator = Object.FindObjectOfType<StrobeGenerator>();
                strobeGenerator.GenerateStrobe(new List<StrobeGeneratorPass>()
                {
                    new StrobeStepGradientPass((int)LightValue.BlueOn, false, 2, Easing.Linear)
                });

                CheckEvent(eventsContainer, 1, 2.5f, (int)EventTypeValue.RingLights, (int)LightValue.BlueOn, new JSONObject
                {
                    ["_color"] = new Color(0, 0.5f, 0.5f)
                });
            }
        }

        [Test]
        public void LightIDChromaStepGradient()
        {
            BeatmapObjectContainerCollection containerCollection = BeatmapObjectContainerCollection.GetCollectionForType(ObjectType.Event);
            if (containerCollection is EventGridContainer eventsContainer)
            {
                Transform root = eventsContainer.transform.root;
                EventPlacement eventPlacement = root.GetComponentInChildren<EventPlacement>();

                IEvent eventA = new V2Event(2, (int)EventTypeValue.RingLights, (int)LightValue.RedOn, new JSONObject
                {
                    ["_color"] = new Color(0, 1, 0)
                });
                IEvent eventB = new V2Event(3, (int)EventTypeValue.RingLights, (int)LightValue.RedOn, new JSONObject
                {
                    ["_color"] = new Color(0, 0, 1)
                });
                IEvent eventC = new V2Event(3, (int)EventTypeValue.RingLights, (int)LightValue.RedOn, new JSONObject
                {
                    ["_lightID"] = 1,
                    ["_color"] = new Color(1, 0, 0)
                });
                IEvent eventD = new V2Event(2, (int)EventTypeValue.RingLights, (int)LightValue.RedOn, new JSONObject
                {
                    ["_lightID"] = 1,
                    ["_color"] = new Color(1, 1, 0)
                });
                IEvent eventE = new V2Event(4, (int)EventTypeValue.RingLights, (int)LightValue.RedOn, new JSONObject
                {
                    ["_lightID"] = new JSONArray()
                    {
                        [0] = 1,
                        [1] = 2
                    },
                    ["_color"] = new Color(1, 0, 1)
                });
                IEvent eventF = new V2Event(3, (int)EventTypeValue.RingLights, (int)LightValue.RedOn, new JSONObject
                {
                    ["_lightID"] = 3,
                    ["_color"] = new Color(0, 1, 1)
                });

                foreach (IEvent IEvent in new IEvent[] { eventA, eventB, eventC, eventD, eventE, eventF })
                {
                    eventPlacement.queuedData = IEvent;
                    eventPlacement.queuedValue = eventPlacement.queuedData.Value;
                    eventPlacement.RoundedTime = eventPlacement.queuedData.Time;
                    eventPlacement.ApplyToMap();
                }

                SelectionController.Select(eventC);
                SelectionController.Select(eventD, true);
                SelectionController.Select(eventE, true);

                StrobeGenerator strobeGenerator = Object.FindObjectOfType<StrobeGenerator>();
                strobeGenerator.GenerateStrobe(new List<StrobeGeneratorPass>()
                {
                    new StrobeStepGradientPass((int)LightValue.BlueOn, false, 2, Easing.Linear)
                });

                // Current _lightID from the first event is used. As eventC is added first here we always get a single light id
                // If this changes in future then update below, this test wasn't really meant to enforce this behaviour
                CheckEvent(eventsContainer, 2, 2.5f, (int)EventTypeValue.RingLights, (int)LightValue.BlueOn, new JSONObject
                {
                    ["_color"] = new Color(1, 0.5f, 0),
                    ["_lightID"] = 1
                });
                CheckEvent(eventsContainer, 6, 3.5f, (int)EventTypeValue.RingLights, (int)LightValue.BlueOn, new JSONObject
                {
                    ["_color"] = new Color(1, 0, 0.5f),
                    ["_lightID"] = 1
                });
            }
        }
    }
}
