import { Box } from "@mui/material";
import "./Board.css";
import { useEffect, useState } from "react";

import {
  putEntityInLayout,
  calculateOverhang,
  canBePlaced,
} from "./layoutHelpers";

const BoardComponent = ({
  board,
  username,
  playerId,
  currentlyPlacing,
  setCurrentlyPlacing,
  rotateShip,
  placeShip,
  placedShips,
}) => {
  const originalCells = board.cells;
  const [cells, setCells] = useState(board.cells);
  const { xLength, yLength } = board;

  useEffect(() => {
    const handleKeyDown = (event) => {
      if (currentlyPlacing != null && event.key === " ") {
        event.preventDefault();
        rotateShip({ button: 2 });
      }
    };

    document.addEventListener("keydown", handleKeyDown);

    return () => {
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [rotateShip]);

  const getCellClassName = (cell) => {
    let ownerColor = "defaultcell";

    if (cell.ownerId === playerId) {
      ownerColor = "purple";
    }

    switch (cell.state) {
      case "DamagedShip":
        return "damagedship";
      case "Sunken":
        return "sunken";
      case "Missed":
        return "missed";
      case "placingShip":
        return "placingShip";
      case "forbidden":
        return "forbidden";
      default:
        return `${ownerColor}`;
    }
  };

  const handleCellClick = (xIndex, yIndex) => {
    console.log(`Clicked cell at position x: ${xIndex}, y: ${yIndex}`);
  };

  const handleMouseOver = (event) => {
    const [x, y] = event.target.id.split("-").map(Number);
    console.log(x, y);
    if (currentlyPlacing) {
      const newPosition = { x, y };
      const newCurrentlyPlacing = {
        ...currentlyPlacing,
        position: newPosition,
      };

      setCurrentlyPlacing(newCurrentlyPlacing);

      const updatedCells = cells.map((row) =>
        row.map((cell) => {
          if (cell.state === "placingShip" || cell.state === "forbidden") {
            return { ...cell, state: 1 };
          }
          return cell;
        })
      );

      if (updatedCells[x][y].ownerId === playerId) {
        if (canBePlaced(playerId, newCurrentlyPlacing, originalCells)) {
          setCells(
            putEntityInLayout(updatedCells, newCurrentlyPlacing, "placingShip")
          );
        } else {
          let forbiddenShip = {
            ...newCurrentlyPlacing,
            length:
              newCurrentlyPlacing.length -
              calculateOverhang(newCurrentlyPlacing, cells, playerId),
          };
          setCells(putEntityInLayout(updatedCells, forbiddenShip, "forbidden"));
        }
      }
    }
  };

  return (
    <Box sx={{ mt: 3 }}>
      <div
        className="board-grid"
        style={{
          gridTemplateColumns: `repeat(${board.xLength}, 40px)`,
          gridTemplateRows: `repeat(${board.yLength}, 40px)`,
        }}
      >
        {Array.from({ length: yLength }).map((_, yIndex) =>
          Array.from({ length: xLength }).map((_, xIndex) => (
            <div
              key={`${xIndex}-${yIndex}`}
              id={`${xIndex}-${yIndex}`}
              className={`cell ${getCellClassName(cells[xIndex][yIndex])}`}
              onClick={() => handleCellClick(xIndex, yIndex)}
              onMouseOver={(event) => handleMouseOver(event)}
            />
          ))
        )}
      </div>
    </Box>
  );
};

export default BoardComponent;
