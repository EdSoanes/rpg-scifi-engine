import { Drawer, Heading } from '@chakra-ui/react'
import { StatsBlock } from '../components/stats'
import { ConditionsBlock, StatesBlock } from '../components/states'
import {
  ActionTemplatesBlock,
  SkillTemplatesBlock,
} from '../components/activities'
import LifeBlock from '../components/life/LifeBlock'
import { GraphStateBlock } from '../components/graph'
import { useSelector } from 'react-redux'
import { selectPlayerCharacter } from '../app/graphState/graphSelectors'
import { useAppSelector } from '../app/hooks'
import { GearBlock } from '../components/gear'
import { TimeBlock } from '../components/time'

//import { isEncounterTime } from '../app/utils/is-encounter-time'

export default function CharacterSheet() {
  //const time = useSelector(selectTime)
  const playerCharacter = useSelector(selectPlayerCharacter)
  const { hands, wearing } = useAppSelector((state) => state.gear)

  return (
    <>
      <Drawer.Root /*open={open} onOpenChange={(e) => setOpen(e.open)}*/>
        <Drawer.Trigger>
          <Heading>{playerCharacter?.name ?? 'Nobody'}</Heading>
        </Drawer.Trigger>
        <Drawer.Content>
          <Drawer.Header></Drawer.Header>
          <Drawer.Body>
            <p>
              Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do
              eiusmod tempor incididunt ut labore et dolore magna aliqua.
            </p>
          </Drawer.Body>
          <Drawer.Footer>
            {/* <Drawer.ActionTrigger asChild>
              <Button variant="outline">Cancel</Button>
            </Drawer.ActionTrigger>
            <Button>Save</Button> */}
          </Drawer.Footer>
          <Drawer.CloseTrigger />
        </Drawer.Content>
      </Drawer.Root>
      <TimeBlock />
      <StatsBlock />
      <StatesBlock />
      <ConditionsBlock />
      <ActionTemplatesBlock />
      <SkillTemplatesBlock />
      <LifeBlock />
      <GearBlock name={'Hands'} container={hands} />
      <GearBlock name={'Wearing'} container={wearing} />
      <GraphStateBlock />
    </>
  )
}
