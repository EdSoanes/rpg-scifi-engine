import { atom, useAtom } from 'jotai'
import {
  Button,
  Code,
  Drawer,
  DrawerBody,
  DrawerCloseButton,
  DrawerContent,
  DrawerFooter,
  DrawerHeader,
  DrawerOverlay,
  Grid,
  Heading,
  Stack,
  StatGroup,
  Tab,
  TabList,
  TabPanel,
  TabPanels,
  Tabs,
  useDisclosure,
} from '@chakra-ui/react'
import React from 'react'
import { splitAtom } from 'jotai/utils'
import { playerCharacterActionsAtom } from '../atoms/playerCharacterActions.atom'
import ActionButton from './ActionButton'
import {
  Action,
  ActionInstance,
  ModSet,
  PropValue,
} from '../../lib/rpg-api/types'
import { playerCharacterAtom } from '../atoms/playerCharacter.atom'
import { StatPanel } from '../stats'
import {
  getActionCost,
  getActionInstance,
  postModSet,
} from '../../lib/rpg-api/fetcher'
import { graphStateAtom } from '../atoms/graphState.atom'

const actionAtomsAtom = splitAtom(playerCharacterActionsAtom)
const selectedActionAtom = atom<Action | null>(null)
const actionInstanceAtom = atom<ActionInstance | null>(null)
const actionCostAtom = atom<ModSet | null>(null)

const reactionsAtom = atom<PropValue | null>(
  (get) => get(playerCharacterAtom)?.reactions ?? null
)

const actionPointsAtom = atom<PropValue | null>((get) => {
  const pc = get(playerCharacterAtom)
  return {
    id: pc?.id,
    value: pc?.currentActionPoints,
    baseValue: pc?.actionPoints,
  } as PropValue
})

const focusPointsAtom = atom<PropValue | null>((get) => {
  const pc = get(playerCharacterAtom)
  return {
    id: pc?.id,
    value: pc?.currentFocusPoints,
    baseValue: pc?.focusPoints,
  } as PropValue
})

const luckPointsAtom = atom<PropValue | null>((get) => {
  const pc = get(playerCharacterAtom)
  return {
    id: pc?.id,
    value: pc?.currentLuckPoints,
    baseValue: pc?.luckPoints,
  } as PropValue
})

function ActionsBlock() {
  const { isOpen, onOpen, onClose } = useDisclosure()
  const [selectedAction, setSelectedAction] = useAtom(selectedActionAtom)
  const [actionInstance, setActionInstance] = useAtom(actionInstanceAtom)
  const [actionCost, setActionCost] = useAtom(actionCostAtom)

  const [playerCharacter] = useAtom(playerCharacterAtom)
  const [graphState, setGraphState] = useAtom(graphStateAtom)

  const onActionButtonClicked = async (action: Action) => {
    if (playerCharacter != null) {
      const res = await getActionInstance(
        playerCharacter?.id,
        playerCharacter?.id,
        action.name,
        0,
        graphState!
      )

      setSelectedAction(action)
      setActionInstance(res)

      if (res) {
        const cost = await getActionCost(
          res.ownerId,
          res.initiatorId,
          res.actionName,
          res.actionNo,
          graphState!
        )

        setActionCost(cost)
      }

      onOpen()
    }
  }

  const onCostButtonClicked = async () => {
    if (actionCost) {
      const res = await postModSet(actionCost, graphState!)
      setGraphState(res)
    }
  }

  const [actionAtoms] = useAtom(actionAtomsAtom)
  return (
    <>
      <Stack w={'100%'}>
        <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
          Action
        </Heading>
        <StatGroup w={'100%'} alignItems={'stretch'}>
          <StatPanel
            propName={'Action Points'}
            propNameAbbr={''}
            propValueAtom={actionPointsAtom}
          />
          <StatPanel
            propName={'Focus Points'}
            propNameAbbr={''}
            propValueAtom={focusPointsAtom}
          />
          <StatPanel
            propName={'Luck Points'}
            propNameAbbr={''}
            propValueAtom={luckPointsAtom}
          />
          <StatPanel
            propName={'Reactions'}
            propNameAbbr={''}
            propValueAtom={reactionsAtom}
          />
        </StatGroup>
        <Grid templateColumns="repeat(6, 1fr)" gap={6}>
          {actionAtoms.map((state, i) => (
            <ActionButton
              key={i}
              actionAtom={state}
              onAction={onActionButtonClicked}
            />
          ))}
        </Grid>
      </Stack>
      <Drawer isOpen={isOpen} size={'lg'} placement="right" onClose={onClose}>
        <DrawerOverlay />
        <DrawerContent>
          <DrawerCloseButton />
          <DrawerHeader>{selectedAction?.name ?? '-'}</DrawerHeader>

          <DrawerBody>
            <Tabs>
              <TabList>
                <Tab>Cost</Tab>
                <Tab>Instance</Tab>
              </TabList>

              <TabPanels>
                <TabPanel>
                  <Code>{JSON.stringify(actionCost, null, 2)}</Code>
                  <Button onClick={onCostButtonClicked}>Act!</Button>
                </TabPanel>
                <TabPanel>
                  <Code>{JSON.stringify(actionInstance, null, 2)}</Code>
                </TabPanel>
              </TabPanels>
            </Tabs>
          </DrawerBody>

          <DrawerFooter>
            <Button variant="outline" mr={3} onClick={onClose}>
              Cancel
            </Button>
            <Button colorScheme="blue">Save</Button>
          </DrawerFooter>
        </DrawerContent>
      </Drawer>
    </>
  )
}

export default ActionsBlock
