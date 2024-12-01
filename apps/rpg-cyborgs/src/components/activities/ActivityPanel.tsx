import React from 'react'
import {
  //Code,
  Stack,
  // Tab,
  // TabList,
  // TabPanel,
  // TabPanels,
  // Tabs,
} from '@chakra-ui/react'
import ActionPanel from './ActionPanel'
import { selectActivity } from '../../app/activity/activitySelectors'
import { useSelector } from 'react-redux'
import { selectActionsStatus } from '../../app/actions/actionTemplatesSelectors'

function ActivityPanel() {

  const activity = useSelector(selectActivity)
  const actionStatus = useSelector(selectActionsStatus)

  return (
    activity && actionStatus === 'loaded' &&
    <Stack w={'100%'}>
      {activity.actions.map((i) => (
        <ActionPanel key={(`${i.name}/${i.actionNo}`)} action={i} />
      ))}
    </Stack>
  )
}

export default ActivityPanel
