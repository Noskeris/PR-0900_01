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
  addShip,
  gameState,
  placeShip,
  playerTurn,
  attackCell
}) => {
  const nameToShipTypeMapping = {
    carrier: 1,
    battleship: 2,
    cruiser: 3,
    submarine: 4,
    destroyer: 5,
  };
  const [originalCells, setOriginalCells] = useState(board.cells);
  const [cells, setCells] = useState(board.cells);
  const { xLength, yLength } = board;

  useEffect(() => {
    console.log("Board updated:", board);
    const deepCopy = (arr) =>
      arr.map((row) => row.map((cell) => ({ ...cell })));
    setCells(deepCopy(board.cells));
    setOriginalCells(deepCopy(board.cells));
  }, [board]);

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
      if (cell.state === 2 && gameState === 2) {
        return (ownerColor = "hasship");
      } else if (cell.state === 2) {
        ownerColor = "hasship";
      } else {
        ownerColor = "purple";
      }
    }

    switch (cell.state) {
      case "hoverOver":
        return "hoverOver";
      case 3:
        return "damagedship";
      case 4:
        return "sunken";
      case 5:
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
    if (gameState === 2) {
      if (canBePlaced(playerId, currentlyPlacing, originalCells)) {
        let endX = currentlyPlacing.position.x;
        let endY = currentlyPlacing.position.y;

        if (currentlyPlacing.orientation === "horizontal") {
          endX = currentlyPlacing.position.x + currentlyPlacing.length - 1;
        } else if (currentlyPlacing.orientation === "vertical") {
          endY = currentlyPlacing.position.y + currentlyPlacing.length - 1;
        }

        const placedShip = {
          shipType: nameToShipTypeMapping[currentlyPlacing.name],
          startX: currentlyPlacing.position.x,
          startY: currentlyPlacing.position.y,
          endX: endX,
          endY: endY,
        };

        addShip(placedShip);
        placeShip(currentlyPlacing);
      }
    } else if( gameState === 3) {
        if(playerTurn === playerId){
          console.log("attacking ", xIndex, yIndex);
          attackCell(xIndex, yIndex);
        }
        else {
          console.log("cant attack now not your turn", xIndex, yIndex);
        }
    }
  };

  const handleMouseOver = (event) => {
    const [x, y] = event.target.id.split("-").map(Number);
    if (gameState === 2) {
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
              putEntityInLayout(
                updatedCells,
                newCurrentlyPlacing,
                "placingShip"
              )
            );
          } else {
            let forbiddenShip = {
              ...newCurrentlyPlacing,
              length:
                newCurrentlyPlacing.length -
                calculateOverhang(newCurrentlyPlacing, cells, playerId),
            };
            setCells(
              putEntityInLayout(updatedCells, forbiddenShip, "forbidden")
            );
          }
        }
      }
    } else if (gameState === 3) {
      const updatedCells = cells.map((row, yIndex) =>
        row.map((cell, xIndex) => {
          if (cell.state === "hoverOver") {
            return { ...cell, state: originalCells[yIndex][xIndex].state };
          }
          return cell;
        })
      );
      if (updatedCells[x][y].ownerId !== playerId) {
        updatedCells[x][y].state = "hoverOver";
        setCells(updatedCells);
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
