'use client'
import React, { useEffect } from 'react'
import { Heading, useColorMode } from '@chakra-ui/react'
import { StatsBlock } from '../components/stats'
import { StatesBlock } from '../components/states'
import { ActionsBlock } from '../components/actions'
import LifeBlock from '../components/life/LifeBlock'
import { GraphStateBlock } from '../components/graph'
import { useSelector } from 'react-redux'
import {
  selectPlayerCharacter,
  selectTime,
} from '../app/graphState/graphSelectors'
import { useAppSelector } from '../app/hooks'
import { GearBlock } from '../components/gear'
import { TimeBlock } from '../components/time'
import { isEncounterTime } from '../app/utils/is-encounter-time'

export default function CharacterSheet() {
  const time = useSelector(selectTime)
  const { colorMode, setColorMode } = useColorMode()
  const playerCharacter = useSelector(selectPlayerCharacter)
  const { hands, wearing } = useAppSelector((state) => state.gear)

  useEffect(() => {
    const timeMode = isEncounterTime(time) ? 'dark' : 'light'
    if (timeMode !== colorMode) {
      setColorMode(timeMode)
    }
  }, [time, colorMode, setColorMode])

  return (
    <>
      <Heading>{playerCharacter?.name ?? 'Nobody'}</Heading>
      <TimeBlock />
      <StatsBlock />
      <StatesBlock />
      <ActionsBlock />
      <LifeBlock />
      <GearBlock name={'Hands'} container={hands} />
      <GearBlock name={'Wearing'} container={wearing} />
      <GraphStateBlock />
    </>
  )
}
