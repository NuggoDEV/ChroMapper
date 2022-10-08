using System.Collections.Generic;
using Beatmap.Enums;
using Beatmap.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beatmap.Containers
{
    public class NoteContainer : ObjectContainer
    {
        private static readonly Color unassignedColor = new Color(0.1544118f, 0.1544118f, 0.1544118f);

        [SerializeField] private GameObject simpleBlock;
        [SerializeField] private GameObject complexBlock;

        [SerializeField] private List<MeshRenderer> noteRenderer;
        [SerializeField] private MeshRenderer bombRenderer;
        [SerializeField] private MeshRenderer dotRenderer;
        [SerializeField] private MeshRenderer arrowRenderer;
        [SerializeField] private SpriteRenderer swingArcRenderer;

        [SerializeField] public INote NoteData;

        public override IObject ObjectData
        {
            get => NoteData;
            set => NoteData = (INote)value;
        }

        public override void Setup()
        {
            base.Setup();

            if (simpleBlock != null)
            {
                simpleBlock.SetActive(Settings.Instance.SimpleBlocks);
                complexBlock.SetActive(!Settings.Instance.SimpleBlocks);

                MaterialPropertyBlock.SetFloat("_Lit", Settings.Instance.SimpleBlocks ? 0 : 1);
                MaterialPropertyBlock.SetFloat("_TranslucentAlpha", Settings.Instance.PastNoteModelAlpha);

                UpdateMaterials();
            }

            SetArcVisible(NoteGridContainer.ShowArcVisualizer);
        }

        internal static Vector3 Directionalize(INote mapNoteData)
        {
            if (mapNoteData is null) return Vector3.zero;
            var cutDirection = mapNoteData.CutDirection;
            var directionEuler = Directionalize(cutDirection);
            if (mapNoteData.CustomDirection != null)
            {
                directionEuler = new Vector3(0, 0, mapNoteData.CustomDirection ?? 0);
            }
            else
            {
                var newMapNoteData = mapNoteData;
                if (newMapNoteData != null && newMapNoteData.AngleOffset != 0)
                {
                    directionEuler += new Vector3(0, 0, newMapNoteData.AngleOffset);
                }
                else
                {
                    if (cutDirection >= 1000) directionEuler += new Vector3(0, 0, 360 - (cutDirection - 1000));
                }
            }

            return directionEuler;
        }

        internal static Vector3 Directionalize(int cutDirection)
        {
            var directionEuler = Vector3.zero;
            switch (cutDirection)
            {
                case (int)NoteCutDirection.Up:
                    directionEuler += new Vector3(0, 0, 180);
                    break;
                case (int)NoteCutDirection.Down:
                    directionEuler += new Vector3(0, 0, 0);
                    break;
                case (int)NoteCutDirection.Left:
                    directionEuler += new Vector3(0, 0, -90);
                    break;
                case (int)NoteCutDirection.Right:
                    directionEuler += new Vector3(0, 0, 90);
                    break;
                case (int)NoteCutDirection.UpRight:
                    directionEuler += new Vector3(0, 0, 135);
                    break;
                case (int)NoteCutDirection.UpLeft:
                    directionEuler += new Vector3(0, 0, -135);
                    break;
                case (int)NoteCutDirection.DownLeft:
                    directionEuler += new Vector3(0, 0, -45);
                    break;
                case (int)NoteCutDirection.DownRight:
                    directionEuler += new Vector3(0, 0, 45);
                    break;
            }

            return directionEuler;
        }

        public void SetDotVisible(bool b)
        {
            dotRenderer.enabled = b;
        }

        public void SetArrowVisible(bool b)
        {
            arrowRenderer.enabled = b;
        }

        public void SetBomb(bool b)
        {
            simpleBlock.SetActive(!b && Settings.Instance.SimpleBlocks);
            complexBlock.SetActive(!b && !Settings.Instance.SimpleBlocks);

            bombRenderer.gameObject.SetActive(b);
            bombRenderer.enabled = b;
        }

        public void SetArcVisible(bool showArcVisualizer)
        {
            if (swingArcRenderer != null) swingArcRenderer.enabled = showArcVisualizer;
        }

        public static NoteContainer SpawnBeatmapNote(INote noteData, ref GameObject notePrefab)
        {
            var container = Instantiate(notePrefab).GetComponent<NoteContainer>();
            container.NoteData = noteData;
            container.transform.localEulerAngles = Directionalize(noteData);
            return container;
        }

        public override void UpdateGridPosition()
        {
            transform.localPosition = (Vector3)NoteData.GetPosition() +
                                      new Vector3(0, 0.5f, NoteData.Time * EditorScaleController.EditorScale);
            transform.localScale = NoteData.GetScale() + new Vector3(0.5f, 0.5f, 0.5f);

            UpdateCollisionGroups();

            MaterialPropertyBlock.SetFloat("_ObjectTime", NoteData.Time);
            SetRotation(AssignedTrack != null ? AssignedTrack.RotationValue.y : 0);
            UpdateMaterials();
        }

        public void SetColor(Color? color)
        {
            MaterialPropertyBlock.SetColor(ObjectContainer.color, color ?? unassignedColor);
            UpdateMaterials();
        }

        internal override void UpdateMaterials()
        {
            foreach (var renderer in noteRenderer) renderer.SetPropertyBlock(MaterialPropertyBlock);
            foreach (var renderer in SelectionRenderers) renderer.SetPropertyBlock(MaterialPropertyBlock);
            bombRenderer.SetPropertyBlock(MaterialPropertyBlock);
        }
    }
}
