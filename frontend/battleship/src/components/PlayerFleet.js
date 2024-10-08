import React from 'react';
import { ReplicaBox } from './ReplicaBox';

export const PlayerFleet = ({
  availableShips,
  selectShip,
  currentlyPlacing,
  playerReady,
  isPlayerReady
}) => {
  let shipsLeft = availableShips
  ?.slice() 
  .sort((a, b) => b.length - a.length)
  .map((ship) => ship.name); 

  let shipReplicaBoxes = shipsLeft?.map((shipName) => (
    <ReplicaBox
      selectShip={selectShip}
      key={shipName}
      isCurrentlyPlacing={currentlyPlacing && currentlyPlacing.name === shipName}
      shipName={shipName}
      availableShips={availableShips}
    />
  ));

  let fleet = (
    <div id="replica-fleet">
      {shipReplicaBoxes}
    </div>
  );

  let playButton = (
    <div id="play-ready">
      <button
        id="play-button"
        onClick={playerReady}
        disabled={isPlayerReady === true}
      >
        {isPlayerReady ? "Waiting for start" : "Ready to play"}
      </button>
    </div>
  );

  return (
    <div id="available-ships">
      <div className="tip-box-title"> Your Ships</div>
      {availableShips?.length > 0 ? fleet : playButton}
    </div>
  );
};