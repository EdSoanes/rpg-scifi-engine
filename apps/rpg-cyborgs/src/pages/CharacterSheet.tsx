import { Heading, Stack } from '@chakra-ui/react'
import { StatsBlock } from '../components/stats'
import { StatesBlock } from '../components/states'
import { ActionTemplatesBlock } from '../components/activities'
import LifeBlock from '../components/life/LifeBlock'
import { GraphStateBlock } from '../components/graph'
import { useSelector } from 'react-redux'
import { selectPlayerCharacter } from '../app/graphState/graphSelectors'
import { useAppSelector } from '../app/hooks'
import { GearBlock } from '../components/gear'
import { TimeBlock } from '../components/time'
import BoxButton from '../components/ui/box-button'
import { useState } from 'react'
import { PiCheckCircleLight, PiCheckCircleFill } from 'react-icons/pi'
//import { isEncounterTime } from '../app/utils/is-encounter-time'

export default function CharacterSheet() {
  //const time = useSelector(selectTime)
  const playerCharacter = useSelector(selectPlayerCharacter)
  const { hands, wearing } = useAppSelector((state) => state.gear)

  const [isOn, setIsOn] = useState<string>('off')

  const onBoxButtonClick = (toggleState: string) => {
    setIsOn(toggleState)
  }
  return (
    <>
      <Heading>{playerCharacter?.name ?? 'Nobody'}</Heading>
      <BoxButton onButtonClicked={onBoxButtonClick} toggle={true}>
        <h1>Button</h1>
        <Stack>
          <div>Some stuff</div>
          <div>Some more stuff</div>
          {isOn == 'on' ? <PiCheckCircleFill /> : <PiCheckCircleLight />}
        </Stack>
      </BoxButton>
      <TimeBlock />
      <StatsBlock />
      <StatesBlock />
      <ActionTemplatesBlock />
      <LifeBlock />
      <GearBlock name={'Hands'} container={hands} />
      <GearBlock name={'Wearing'} container={wearing} />
      <GraphStateBlock />
    </>
  )
}
