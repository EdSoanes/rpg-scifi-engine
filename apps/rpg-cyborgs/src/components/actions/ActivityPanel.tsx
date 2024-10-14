import { atom, Atom, useAtom } from 'jotai'
import React, { useMemo } from 'react'
import { ActionInstance, Activity } from '../../lib/rpg-api/types'
import {
  Code,
  Stack,
  Tab,
  TabList,
  TabPanel,
  TabPanels,
  Tabs,
} from '@chakra-ui/react'
import ActionInstancePanel from './ActionInstancePanel'

// import { graphStateAtom } from '../atoms/graphState.atom'
// import { getActionAct, getActionOutcome } from '../../lib/rpg-api/fetcher'
// import ArgForm from './forms/ArgForm'

// const costAtom = atom<ModSet | null>(null)
// const actAtom = atom<ModSet | null>(null)
// const outcomeAtom = atom<ModSet[] | null>(null)

export declare interface ActivityPanelProps {
  activityAtom: Atom<Activity | undefined>
}

function ActivityPanel(props: ActivityPanelProps) {
  const actionInstancesAtom = useMemo(
    () =>
      atom<ActionInstance[]>((get) => {
        return get(props.activityAtom)?.actionInstances ?? []
      }),
    [props.activityAtom]
  )

  const actionInstanceAtom = useMemo(
    () =>
      atom<ActionInstance | undefined>((get) => {
        return get(props.activityAtom)?.actionInstance
      }),
    [props.activityAtom]
  )

  const [activity] = useAtom(props.activityAtom)
  const [actionInstance] = useAtom(actionInstanceAtom)
  const [actionInstances] = useAtom(actionInstancesAtom)

  // const [cost, setCost] = useAtom(costAtom)
  // const [act, setAct] = useAtom(actAtom)
  // const [outcome, setOutcome] = useAtom(outcomeAtom)

  // const [graphState, setGraphState] = useAtom(graphStateAtom)

  // const onCostClicked = async (argValues: {
  //   [key: string]: string | null | undefined
  // }) => {
  //   const cost = await getActionCost(actionInstance!, argValues, graphState!)
  //   setCost(cost)
  // }

  // const onActClicked = async (argValues: {
  //   [key: string]: string | null | undefined
  // }) => {
  //   const act = await getActionAct(actionInstance!, argValues, graphState!)
  //   setAct(act)
  // }

  // const onOutcomeClicked = async (argValues: {
  //   [key: string]: string | null | undefined
  // }) => {
  //   const outcome = await getActionOutcome(
  //     actionInstance!,
  //     argValues,
  //     graphState!
  //   )
  //   setOutcome(outcome)
  // }

  return (
    <Stack w={'100%'}>
      {actionInstances.map((i) => (
        <ActionInstancePanel />
      ))}
    </Stack>
  )
}

export default ActivityPanel
