using System;
using UnityEngine;
using UnityEngine.Tilemaps;
public enum Tetromino //블록의 종류
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z
}
[Serializable]
public struct TetrominoData //블록 데이터
{
    public Tetromino tetromino;//무슨 블록인지
    public Tile tile;//어떤 타일인지
    public Vector2Int[] cells { get; private set; }//셀들의 정보
    public Vector2Int[,] wallkicks { get; private set; }//벽킥의 정보들

    public void Initialize()//초기화
    {
        this.cells = Data.Cells[this.tetromino];//셀 데이터 복사
        this.wallkicks = Data.WallKicks[this.tetromino];//벽킥 데이터 복사
    }
}
