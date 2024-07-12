import { getPlayerCharacter } from "@/lib/api/fetcher";
import { PlayerCharacter } from "@/lib/api/types";

export default async function Home() {
  const graphState = await getPlayerCharacter(
    "badb499a-b215-45c8-adf1-0f0908f876f6"
  );

  console.log("GRAPHSTATE", graphState.data);
  const pc = !graphState.error
    ? (graphState.data.entities.find(
        (x) => x.id == graphState.data.contextId
      ) as PlayerCharacter)
    : null;

  return (
    <div>
      {graphState.error ? (
        <div>There was an error</div>
      ) : (
        <pre>
          <code>{JSON.stringify(pc, undefined, 2)}</code>
        </pre>
      )}
    </div>
  );
}
