import React from 'react'
import { Heading, Stack } from '@chakra-ui/react'
import { BodyPart } from '../../../lib/rpg-api/cyborg-types'

export interface BodyPartPanelProps {
  bodyPart?: BodyPart
}
function BodyPartPanel(props: BodyPartPanelProps) {
  const { bodyPart } = props
  return (
    <Stack direction={'column'}>
      <Heading size={'md'}>{bodyPart?.name}</Heading>{' '}
      <span>{bodyPart?.bodyPartType}</span>
    </Stack>
  )
}

export default BodyPartPanel
