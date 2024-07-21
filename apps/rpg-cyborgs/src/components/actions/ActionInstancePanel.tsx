import { atom, Atom, useAtom } from 'jotai'
import React from 'react'
import { ActionInstance, ActionModSet, ModSet } from '../../lib/rpg-api/types'
import { Code, Tab, TabList, TabPanel, TabPanels, Tabs } from '@chakra-ui/react'

import { graphStateAtom } from '../atoms/graphState.atom'
import {
  getActionAct,
  getActionCost,
  getActionOutcome,
} from '../../lib/rpg-api/fetcher'
import ArgForm from './forms/ArgForm'

const costAtom = atom<ModSet | null>(null)
const actAtom = atom<ActionModSet | null>(null)
const outcomeAtom = atom<ModSet[] | null>(null)

export declare interface ActionInstancePanelProps {
  actionInstanceAtom: Atom<ActionInstance | null>
}

function ActionInstancePanel(props: ActionInstancePanelProps) {
  const [cost, setCost] = useAtom(costAtom)
  const [act, setAct] = useAtom(actAtom)
  const [outcome, setOutcome] = useAtom(outcomeAtom)

  const [graphState, setGraphState] = useAtom(graphStateAtom)
  const [actionInstance] = useAtom(props.actionInstanceAtom)

  const onCostClicked = async (argValues: {
    [key: string]: string | null | undefined
  }) => {
    const cost = await getActionCost(actionInstance!, argValues, graphState!)
    setCost(cost)
  }

  const onActClicked = async (argValues: {
    [key: string]: string | null | undefined
  }) => {
    const act = await getActionAct(actionInstance!, argValues, graphState!)
    setAct(act)
  }

  const onOutcomeClicked = async (argValues: {
    [key: string]: string | null | undefined
  }) => {
    const outcome = await getActionOutcome(
      actionInstance!,
      argValues,
      graphState!
    )
    setOutcome(outcome)
  }

  return (
    <Tabs>
      <TabList>
        <Tab>Cost</Tab>
        <Tab>Act</Tab>
        <Tab>Outcome</Tab>
        <Tab>Instance</Tab>
      </TabList>

      <TabPanels>
        <TabPanel>
          <ArgForm argSet={actionInstance!.costArgs} onSubmit={onCostClicked} />
          <Code>{JSON.stringify(cost, null, 2)}</Code>
        </TabPanel>
        <TabPanel>
          <ArgForm argSet={actionInstance!.actArgs} onSubmit={onActClicked} />
          <Code>{JSON.stringify(act, null, 2)}</Code>
        </TabPanel>
        <TabPanel>
          <ArgForm
            argSet={actionInstance!.outcomeArgs}
            onSubmit={onOutcomeClicked}
          />
          <Code>{JSON.stringify(outcome, null, 2)}</Code>
        </TabPanel>
        <TabPanel>
          <Code>{JSON.stringify(graphState, null, 2)}</Code>
        </TabPanel>
      </TabPanels>
    </Tabs>
  )
}

export default ActionInstancePanel
