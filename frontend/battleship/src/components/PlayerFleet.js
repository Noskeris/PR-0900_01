import React from 'react';
import { ReplicaBox } from './ReplicaBox';

export const PlayerFleet = ({
  availableShips,
  selectShip,
  currentlyPlacing,
  readyToPlay,
}) => {
  let shipsLeft = availableShips.map((ship) => ship.name);

  let shipReplicaBoxes = shipsLeft.map((shipName) => (
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
      <button id="play-button" onClick={readyToPlay}>
        Ready to play
      </button>
    </div>
  );

  return (
    <div id="available-ships">
      <div className="tip-box-title"> Your Ships</div>
      {availableShips.length > 0 ? fleet : playButton}
    </div>
  );
};